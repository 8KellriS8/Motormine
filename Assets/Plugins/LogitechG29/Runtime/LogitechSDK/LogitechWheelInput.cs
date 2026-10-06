using LogitechG29.Runtime.LogitechSDK;
using System.Runtime.InteropServices;
using UnityEngine;

public class LogitechWheelInput : MonoBehaviour
{
    // Значения, которые мы будем передавать в физику машины
    [Header("Output Values")]
    public float steering = 0f; // От -1.0 (влево) до 1.0 (вправо)
    public float gas = 0f;      // От 0.0 (отпущено) до 1.0 (полный газ)
    public float brake = 0f;    // От 0.0 (отпущено) до 1.0 (полный тормоз)

    void Start()
    {
        // 1. Инициализируем контроллер Logitech (true - режим работы в фоне)
        bool initSuccess = LogitechGsdk.LogiSteeringInitialize(true);

        if (initSuccess)
        {
            Debug.Log("Logitech SDK успешно инициализирован!");
        }
        else
        {
            Debug.LogError("Не удалось инициализировать Logitech SDK. Проверьте, запущен ли Logitech G Hub!");
        }
    }

    void Update()
    {
        // 1. Проверяем, инициализирован ли SDK и подключен ли руль (устройство 0)
        if (LogitechGsdk.LogiUpdate() && LogitechGsdk.LogiIsConnected(0))
        {
            // ЧИТАЕМ ДАННЫЕ С РУЛЯ
            System.IntPtr ptr = LogitechGsdk.LogiGetStateENGINES(0);
            if (ptr != System.IntPtr.Zero)
            {
                LogitechGsdk.Dijoystate2Engines rec =
                    (LogitechGsdk.Dijoystate2Engines)System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, typeof(LogitechGsdk.Dijoystate2Engines));

                steering = (float)rec.lX / 32768f;
                gas = 1f - ((float)(rec.lY + 32768) / 65535f);
                brake = 1f - ((float)(rec.lRz + 32768) / 65535f);

                gas = Mathf.Clamp01(gas);
                brake = Mathf.Clamp01(brake);
            }
        }
        else
        {
            // РУЛЬ НЕ ПОДКЛЮЧЕН — ЧИТАЕМ КЛАВИАТУРУ (WASD / Стрелочки)

            // Поворот: A/D или Стрелки Влево/Вправо (дает значение от -1.0 до 1.0)
            steering = Input.GetAxis("Horizontal");

            // Газ: W или Стрелка Вверх (если нажата, плавно даем 1.0, иначе 0.0)
            float verticalInput = Input.GetAxis("Vertical");
            if (verticalInput > 0)
            {
                gas = verticalInput;
                brake = 0f;
            }
            // Тормоз: S или Стрелка Вниз
            else if (verticalInput < 0)
            {
                brake = Mathf.Abs(verticalInput);
                gas = 0f;
            }
            else
            {
                gas = 0f;
                brake = 0f;
            }
        }
    }

    void OnApplicationQuit()
    {
        // 5. Обязательно освобождаем память при закрытии игры
        LogitechGsdk.LogiSteeringShutdown();
    }
}