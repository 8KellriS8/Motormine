using UnityEngine;
using System.Threading;
using Futurift.DataSenders;
using Futurift.Options;

public class FuturiftTelemetryBridge : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public Rigidbody vehicleRigidbody;

    [Header("Futurift Connection Settings")]
    public int comPortNumber = 3;

    private ComPortSender futuriftSender;
    private Vector3 lastVelocity;

    // Переменные для обмена данными между потоками
    private byte[] dataToSender = new byte[4] { 128, 128, 128, 128 };
    private Thread ioThread;
    private bool isRunning = false;
    private readonly object lockObject = new object(); // Для безопасности потоков

    void Start()
    {
        if (vehicleRigidbody == null) vehicleRigidbody = GetComponent<Rigidbody>();

        ComPortOptions options = new ComPortOptions();
        options.comPort = comPortNumber;

        futuriftSender = new ComPortSender(options);

        try
        {
            futuriftSender.Start();
            Debug.Log($"[Futurift] Порт COM{comPortNumber} успешно открыт.");

            // Запускаем фоновый поток для отправки данных
            isRunning = true;
            ioThread = new Thread(SendPacketsWorker);
            ioThread.IsBackground = true;
            ioThread.Start();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Futurift] Не удалось открыть порт: {e.Message}");
        }
    }

    void FixedUpdate()
    {
        if (vehicleRigidbody == null || futuriftSender == null || !futuriftSender.IsConnected)
            return;

        // --- РАСЧЕТ ТЕЛЕМЕТРИИМАШИНЫ (в основном потоке) ---
        float pitch = vehicleRigidbody.transform.localEulerAngles.x;
        float roll = vehicleRigidbody.transform.localEulerAngles.z;

        if (pitch > 180) pitch -= 360;
        if (roll > 180) roll -= 360;

        Vector3 currentVelocity = vehicleRigidbody.linearVelocity;
        Vector3 acceleration = (currentVelocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = currentVelocity;

        Vector3 localAcceleration = vehicleRigidbody.transform.InverseTransformDirection(acceleration);
        float surgeG = localAcceleration.z / 9.81f;
        float swayG = localAcceleration.x / 9.81f;

        byte pitchByte = (byte)Mathf.Clamp(128 + (pitch * 4f), 0, 255);
        byte rollByte = (byte)Mathf.Clamp(128 + (roll * 4f), 0, 255);
        byte surgeByte = (byte)Mathf.Clamp(128 + (surgeG * 80f), 0, 255);
        byte swayByte = (byte)Mathf.Clamp(128 + (swayG * 80f), 0, 255);

        // Безопасно обновляем массив данных для фонового потока
        lock (lockObject)
        {
            dataToSender[0] = pitchByte;
            dataToSender[1] = rollByte;
            dataToSender[2] = surgeByte;
            dataToSender[3] = swayByte;
        }
    }

    // ЭТОТ МЕТОД РАБОТАЕТ В ОТДЕЛЬНОМ ПОТОКЕ И НЕ ТОРМОЗИТ ИГРУ
    private void SendPacketsWorker()
    {
        byte[] localBuffer = new byte[4];

        while (isRunning)
        {
            if (futuriftSender != null && futuriftSender.IsConnected)
            {
                // Копируем данные из общего буфера
                lock (lockObject)
                {
                    System.Array.Copy(dataToSender, localBuffer, 4);
                }

                try
                {
                    // Отправка в COM-порт
                    futuriftSender.SendData(localBuffer);
                }
                catch (System.Exception)
                {
                    // Игнорируем таймауты в фоне, чтобы не спамить в консоль
                }
            }

            // Ограничиваем частоту отправки до ~30 Гц (каждые 33 миллисекунды),
            // чтобы дать Windows и платформе передышку
            Thread.Sleep(33);
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;

        // Мягко закрываем поток
        if (ioThread != Thread.CurrentThread && ioThread != null && ioThread.IsAlive)
        {
            ioThread.Join(500);
        }

        if (futuriftSender != null && futuriftSender.IsConnected)
        {
            futuriftSender.Stop();
            Debug.Log("[Futurift] Порт закрыт и поток остановлен.");
        }
    }
}