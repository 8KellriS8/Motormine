using UnityEngine;

public class CarController : MonoBehaviour
{
    public LogitechWheelInput wheelInput; // —сылка на наш скрипт ввода

    void Update()
    {
        // ѕередаем данные в физический движок машины
        float currentSteer = wheelInput.steering;
        float currentGas = wheelInput.gas;

        ApplyPhysicsToWheels(currentSteer, currentGas);
    }
    public void ApplyPhysicsToWheels(float currentSteer, float currentGas)
    {
        
    }
}