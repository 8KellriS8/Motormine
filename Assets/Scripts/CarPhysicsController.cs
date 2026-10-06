using UnityEngine;

public class CarPhysicsController : MonoBehaviour
{
    [Header("Ссылки на другие скрипты")]
    // Перетащите сюда ваш объект LogitechManager
    public LogitechWheelInput wheelInput;

    [Header("Физические колеса (Wheel Colliders)")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Визуальные колеса (3D меши для вращения)")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    [Header("Настройки машины")]
    public float maxMotorTorque = 1500f; // Мощность мотора
    public float maxBrakeTorque = 3000f; // Сила тормозов
    public float maxSteeringAngle = 35f; // Максимальный угол поворота колес

    void FixedUpdate()
    {
        if (wheelInput == null) return;

        // 1. Считываем данные из вашего скрипта ввода (где клавиатура/руль)
        float currentSteer = wheelInput.steering;
        float currentGas = wheelInput.gas;
        float currentBrake = wheelInput.brake;

        // 2. Управляем поворотом (передние колеса)
        float steerAngle = currentSteer * maxSteeringAngle;
        if (frontLeftWheel != null) frontLeftWheel.steerAngle = steerAngle;
        if (frontRightWheel != null) frontRightWheel.steerAngle = steerAngle;

        // 3. Управляем газом (задний привод для примера, можно на все 4 колеса)
        float motorForce = currentGas * maxMotorTorque;
        if (rearLeftWheel != null) rearLeftWheel.motorTorque = motorForce;
        if (rearRightWheel != null) rearRightWheel.motorTorque = motorForce;

        // 4. Управляем торможением (на все колеса)
        float brakeForce = currentBrake * maxBrakeTorque;
        ApplyBraking(brakeForce);

        // 5. Визуально вращаем 3D-модели колес, чтобы они крутились и поворачивались
        UpdateWheelVisuals(frontLeftWheel, frontLeftTransform);
        UpdateWheelVisuals(frontRightWheel, frontRightTransform);
        UpdateWheelVisuals(rearLeftWheel, rearLeftTransform);
        UpdateWheelVisuals(rearRightWheel, rearRightTransform);
    }

    private void ApplyBraking(float force)
    {
        if (frontLeftWheel != null) frontLeftWheel.brakeTorque = force;
        if (frontRightWheel != null) frontRightWheel.brakeTorque = force;
        if (rearLeftWheel != null) rearLeftWheel.brakeTorque = force;
        if (rearRightWheel != null) rearRightWheel.brakeTorque = force;
    }

    // Метод синхронизирует положение физического коллайдера и его 3D-модели
    private void UpdateWheelVisuals(WheelCollider collider, Transform visualTransform)
    {
        if (collider == null || visualTransform == null) return;

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualTransform.position = position;
        visualTransform.rotation = rotation;
    }
}

