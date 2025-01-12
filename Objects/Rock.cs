using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : InteractableObject
{
    [Header("Rock Throwing Properties")]
    //new
    [SerializeField] private float maxThrowForce = 20f; // Maximum force
    [SerializeField] private float minThrowForce = 5f; // Minimum force
    [SerializeField] private float angleAdjustmentSpeed = 2f; // Speed of angle adjustment
    [SerializeField] private float forceAdjustmentSpeed = 1f; // Speed of force adjustment
    //end new
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float maxThrowDistance = 20f;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private LineRenderer trajectoryLineRenderer;
    [SerializeField] private int trajectoryPointCount = 20;
    [SerializeField] private float trajectorySimulationTime = 2f;
    [SerializeField] private Camera playerCamera;
    private MovimentarJogador player;

    [Header("Attachment Points")]
    [SerializeField] private Transform rightHandAttachPoint;

    public float rockDrainRate = 1f;
    public float maxCharge = 5f;

    private bool isAiming = false;
    [SerializeField] private Vector3 throwDirection = Vector3.forward; // Default direction (new)
    [SerializeField] private float currentThrowDistance = 0f;

    private void Start()
    {
        if(player == null)
        {
            player = GameObject.Find("Jogador").GetComponent<MovimentarJogador>();
        }

        // Find the TrajectoryLineRenderer component
        trajectoryLineRenderer = GameObject.Find("TrajectoryLineRenderer")?.GetComponent<LineRenderer>();
        if (trajectoryLineRenderer == null)
        {
            Debug.LogError("TrajectoryLineRenderer object not found. Please create and assign it as a child of the player object.");
            enabled = false;
        }
        else
        {
            trajectoryLineRenderer.enabled = false;
        }

        if (rightHandAttachPoint == null) rightHandAttachPoint = GameObject.Find("Jogador").GetComponentInChildren<Transform>(true);

        // Assign the player's main camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (isAiming)
        {
            // Update throw direction based on mouse movement
            //throwDirection = player.transform.forward;

            //new
            // Adjust the throw direction based on mouse movement
            AdjustThrowDirection();

            // Adjust the throw force based on mouse movement
            AdjustThrowForce();
            //endnew

            // Adjust throw distance based on mouse movement
            //currentThrowDistance = Mathf.Clamp(currentThrowDistance + Input.mouseScrollDelta.y * 2f, 0f, maxThrowDistance);

            UpdateTrajectoryVisualization(player.transform.position);
        }
    }

    private void AdjustThrowDirection()
    {
        throwDirection = player.transform.forward;
        Debug.Log(throwDirection);
        // Get horizontal mouse movement
        float mouseX = Input.GetAxis("Mouse X");

        // Rotate throw direction around the Y-axis based on mouseX
        Quaternion rotation = Quaternion.Euler(0f, mouseX * angleAdjustmentSpeed, 0f);
        throwDirection = rotation * throwDirection;

        // Ensure the throwDirection remains normalized
        throwDirection.Normalize();
    }

    private void AdjustThrowForce()
    {
        // Get vertical mouse movement
        float mouseY = Input.GetAxis("Mouse Y");

        // Adjust the throw force based on mouseY
        throwForce += mouseY * forceAdjustmentSpeed;

        // Clamp the throw force between min and max values
        throwForce = Mathf.Clamp(throwForce, minThrowForce, maxThrowForce);
    }

    public override void OnEquip(MovimentarJogador player, float rocks)
    {
        currentCharge = rocks;
        //player.GetComponentInChildren<Animator>().SetTrigger("HoldRock");
    }

    public override float OnUnequip(MovimentarJogador player)
    {
        // player.GetComponentInChildren<Animator>().SetTrigger("HoldTorch");
        return currentCharge;
    }

    public override void OnUse(MovimentarJogador player)
    {
        // Initiate aiming
        if (!isAiming)
        {
            isAiming = true;
            trajectoryLineRenderer.enabled = true;

            //new
            // Initialize throwDirection to forward direction of the player
            throwDirection = player.transform.forward;
        }

        // Calculate and update throw direction based on camera
        //throwDirection = player.transform.forward;
        //(player/*.transform.position);
    }

    public override void OnSecondaryUse(MovimentarJogador player)
    {
        // Toggle aiming off
        if (isAiming)
        {
            isAiming = false;
            trajectoryLineRenderer.enabled = false;
        }
    }

    private void UpdateTrajectoryVisualization(Vector3 startPosition/*MovimentarJogador player*/)
    {
        // TESTING
        //Vector3 startPos = rightHandAttachPoint.position;

        Vector3[] positions = new Vector3[trajectoryPointCount];

        float timeStep = trajectorySimulationTime / trajectoryPointCount;

        for (int i = 0; i < trajectoryPointCount; i++)
        {
            float time = i * timeStep;
            Vector3 point = CalculateTrajectoryPoint(startPosition, throwDirection, time);
            /*positions[i] = startPos + (throwDirection * currentThrowDistance * time) +
                       (0.5f * Physics.gravity * time * time);*/
            //
            positions[i] = point;
        }
        trajectoryLineRenderer.transform.position = startPosition;
        trajectoryLineRenderer.transform.rotation = Quaternion.LookRotation(player.transform.forward);
        trajectoryLineRenderer.positionCount = trajectoryPointCount;
        trajectoryLineRenderer.SetPositions(positions);

        /*Vector3 startPosition = rightHandAttachPoint.position; //player.transform.position + player.transform.forward * 1f; // Offset the start position slightly in front of the player
        Vector3[] positions = new Vector3[trajectoryPointCount];
        float gravity = Physics.gravity.magnitude;
        float timeStep = trajectorySimulationTime / trajectoryPointCount;

        for (int i = 0; i < trajectoryPointCount; i++)
        {
            float time = i * timeStep;
            Vector3 point = CalculateTrajectoryPoint(startPosition, player.transform.forward, time, currentThrowDistance);
            positions[i] = point;
        }

        trajectoryLineRenderer.transform.position = startPosition;
        trajectoryLineRenderer.transform.rotation = Quaternion.LookRotation(player.transform.forward);
        trajectoryLineRenderer.positionCount = trajectoryPointCount;
        trajectoryLineRenderer.SetPositions(positions);*/
    }

    private Vector3 CalculateTrajectoryPoint(Vector3 startPos, Vector3 direction, float time/*, float distance*/)
    {
        float gravity = Physics.gravity.magnitude;
        return startPos
            + direction * throwForce * time /* * (distance / maxThrowDistance)*/
            + 0.5f * Physics.gravity * time * time;
    }

    public void ThrowRock(MovimentarJogador player)
    {
        if (!isAiming) return;

        // Instantiate rock projectile
        GameObject thrownRock = Instantiate(
            rockPrefab,
            player.transform.position + player.transform.forward,
            Quaternion.LookRotation(throwDirection)
        );

        // Apply force to rock
        Rigidbody rockRb = thrownRock.GetComponent<Rigidbody>();
        if (rockRb != null)
        {
            rockRb.useGravity = true;
            rockRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Thrown rock is missing Rigidbody!");
        }

        // Trigger throw animation
        player.GetComponentInChildren<Animator>().SetTrigger("ThrowRock");

        // Reduce rock charge
        currentCharge--;

        // Disable aim and trajectory
        isAiming = false;
        trajectoryLineRenderer.enabled = false;

        // Destroy this rock after throwing if no charge left
        if (currentCharge <= 0)
        {
            Destroy(gameObject);
        }
    }
}