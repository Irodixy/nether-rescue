using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : InteractableObject
{
    [Header("Torch Specific Properties")]
    [SerializeField] private Light torchLight;
    [SerializeField] private float lightIntensity = 5f;
    [SerializeField] private float batteryDrainRate = 1f;
    [SerializeField] private float maxCharge = 100f;
    [SerializeField] private float coneRange = 10f; // Maximum range of the cone
    [SerializeField] private float coneAngle = 45f; // Angle of the cone in degrees
    [SerializeField] private LayerMask enemyLayer; // Layer to filter enemies

    private bool isLightOn = false;
    [SerializeField] private bool isAiming = false;

    public override void OnEquip(MovimentarJogador player, float fuel)
    {
        currentCharge = fuel;
        player.GetComponentInChildren<Animator>().SetTrigger("HoldTorch");
        TurnOnLight();
    }

    public override float OnUnequip(MovimentarJogador player)
    {
        player.GetComponentInChildren<Animator>().ResetTrigger("HoldTorch");
        TurnOffLight();
        return currentCharge;
    }

    public override void OnUse(MovimentarJogador player)
    {
        isAiming = false;
        player.GetComponentInChildren<Animator>().SetBool("AimingTorch", isAiming);
    }

    public override void OnSecondaryUse(MovimentarJogador player)
    {
        // Aim torch as weapon
        isAiming = true;
        player.GetComponentInChildren<Animator>().SetBool("AimingTorch", isAiming);

        // Apply AoE logic
        ApplyTorchEffect(player.transform);
    }

    private void ApplyTorchEffect(Transform playerTransform)
    {
        Collider[] hitColliders = Physics.OverlapSphere(playerTransform.position, coneRange, enemyLayer);

        foreach (var hitCollider in hitColliders)
        {
            Vector3 directionToEnemy = (hitCollider.transform.position - playerTransform.position).normalized;
            float angleToEnemy = Vector3.Angle(playerTransform.forward, directionToEnemy);

            if (angleToEnemy <= coneAngle / 2)
            {
                // Call the enemy's repel behavior
                var enemy = hitCollider.GetComponent<EnemyStalkerController>();
                if (enemy != null)
                {
                    Debug.Log("got to enemy");
                    enemy.RetreatFromTorch(playerTransform.position);
                }
            }
        }
    }

    private void TurnOnLight()
    {
        if (torchLight != null)
        {
            torchLight.enabled = true;
            isLightOn = true;
        }
    }

    private void TurnOffLight()
    {
        if (torchLight != null)
        {
            torchLight.enabled = false;
            isLightOn = false;
        }
    }

    private void Update()
    {
        // Battery drain when light is on
        if (isLightOn)
        {
            if (currentCharge > 0)
            {
                currentCharge -= batteryDrainRate * Time.deltaTime;
            }
            else
            {
                TurnOffLight();
            }
        }
        Reset();
    }

    public void Recharge()
    {
        currentCharge += 50f;
        if (currentCharge > 100) currentCharge = 100;
    }

    private void Reset()
    {

    }
}
