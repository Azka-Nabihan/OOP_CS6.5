using System;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement With Time")]
    // Kecepatan maksimum pada sumbu x dan y
    [SerializeField] Vector2 maxSpeed = new(10f, 10f);
    // Waktu yang diperlukan untuk mencapai kecepatan penuh
    [SerializeField] Vector2 timeToFullSpeed = new(2f, 2f);
    // Waktu yang diperlukan untuk berhenti sepenuhnya
    [SerializeField] Vector2 timeToStop = new(2.5f, 2.5f);
    // Batas kecepatan minimum agar Player dianggap berhenti
    [SerializeField] Vector2 stopClamp = new(2.5f, 2.5f);
    

    [Header("Movement Calculation")]
    // Arah gerakan Player berdasarkan input
    Vector2 moveDirection;
    // Kecepatan gerakan yang diberikan saat Player bergerak
    Vector2 moveVelocity;
    // Gaya gesek saat Player bergerak
    Vector2 moveFriction;
    // Gaya gesek saat Player berhenti
    Vector2 stopFriction;
    // Komponen Rigidbody2D dari Player untuk mengatur physics
    Vector2 ppos;

    [Header("Player Components")]
    Rigidbody2D rb;

    // Fungsi ini dipanggil sekali saat object diinisialisasi
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        /* 
        
            This calculation is derived from physics 2d motion and I got this from:
            https://www.youtube.com/watch?v=hG9SzQxaCm8&t=0s
        
         */
        moveVelocity = 2.0f * maxSpeed / timeToFullSpeed;
        moveFriction = -2.0f * maxSpeed / (timeToFullSpeed * timeToFullSpeed);
        stopFriction = -2.0f * maxSpeed / (timeToStop * timeToStop);
    }

    public void Move()
    {
        /* 
            Get input from player's keyboard to determine which direction the player wants to move.
                - RAW is used here because I want the input to be either -1, 0, or 1 for both axis.
                - And the vector is normalized so that the diagonal movement doesnt have a different movement speed from linear movement. You can search this on Youtube which explains the concept better (2D dropdown fix diagonal movement).
         */
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // The player's velocity is being added overtime and directed by moveDirection above.
        rb.velocity += moveVelocity * Time.deltaTime * moveDirection;

        // Limit the player's velocity so that it will only reach maxSpeed and not further.
        rb.velocity = new(Mathf.Clamp(rb.velocity.x, -maxSpeed.x, maxSpeed.x), Mathf.Clamp(rb.velocity.y, -maxSpeed.y, maxSpeed.y));

        // This represents DECCELERATION using friction, when an object move
        // the force of the motion will have a friction that oppose the move force.
        // The value of the friction is determined using a function below. 
        rb.velocity -= GetFriction() * Time.deltaTime;

        // Both of these if are used to clamp the stop movement, if the
        // velocity is less than stopClamp, then just stop the player instead of
        // waiting for the friction to do it.
        // 
        // Other condition was related to moveBound(), we dont want any velocity
        // if the player already reach the screen bound
        if (Math.Abs(rb.velocity.x) < stopClamp.x && moveDirection.x == 0 || moveDirection.x > -0 && ppos.x >= 0.99f || moveDirection.x < 0 && ppos.x <= 0.01f)
            rb.velocity = new(0, rb.velocity.y);

        if (Math.Abs(rb.velocity.y) < stopClamp.y && moveDirection.y == 0 || moveDirection.y > -0 && ppos.y >= 0.95f || moveDirection.y < 0 && ppos.y <= -0.01f)
            rb.velocity = new(rb.velocity.x, 0);
    }

    public Vector2 GetFriction()
    {
        Vector2 totalFriction = Vector2.zero;

        // The friction is determined based on where the direction of the player
        // wants to go.

        // If the direction is positive, then the friction must be negative
        if (moveDirection.x > 0)
            totalFriction.x = moveFriction.x;

        // If the direction is negative, then the friction must be positive
        else if (moveDirection.x < 0)
            totalFriction.x = -moveFriction.x;

        // If the player doesnt want to move, but
        // the player is still moving
        // that means the player wants to stop
        // And we also check the movement (velocity) if its on negative or positive direction

        // So we check if direction is 0 (player not pressing input and velocity is not added)
        // and the velocity is positive (user going forward)
        // that means the friction needs to be negative

        // This logic is different from above because we use player's velocity
        // and the player's velocity is no longer being added, so we need to
        // actually match the player's velocity
        else if (moveDirection.x == 0 && rb.velocity.x > 0)
            totalFriction.x = -stopFriction.x;
        else if (moveDirection.x == 0 && rb.velocity.x < 0)
            totalFriction.x = stopFriction.x;
        else
            totalFriction.x = 0;

        // The logic for y axis (vertical) is the same as above (x axis or horizontal)
        if (moveDirection.y > 0)
            totalFriction.y = moveFriction.y;
        else if (moveDirection.y < 0)
            totalFriction.y = -moveFriction.y;
        else if (moveDirection.y == 0 && rb.velocity.y > 0)
            totalFriction.y = -stopFriction.y;
        else if (moveDirection.y == 0 && rb.velocity.y < 0)
            totalFriction.y = stopFriction.y;
        else
            totalFriction.y = 0;

        return totalFriction;
    }

    /* 
    
        Limit the player's position so that the player doesnt go
        off screen.

        the worldToViewportPoint is used to transform the player's in the game
        position into a viewport point which is a number between 0 and 1
        that relative to the screen or camera.

        If the number is greater than 1 that means user is offscreen to the right or up
        If the number is less than 0 that means user is offscreen to the left or bottom

     */
    public void MoveBound()
    {
        // Converting player's position to 0 and 1
        // relative to camera
        ppos = Camera.main.WorldToViewportPoint(transform.position);

        // limit the position so that it doesnt go off screen
        ppos.x = Mathf.Clamp(ppos.x, 0.01f, 0.99f);
        ppos.y = Mathf.Clamp(ppos.y, -0.01f, 0.95f);

        // Set the player's position
        // With the converted back (viewport to world)
        transform.position = Camera.main.ViewportToWorldPoint(ppos) + new Vector3(0, 0, 10);
    }

    public bool IsMoving()
    {
        if (moveDirection.magnitude != 0)
            return true;

        return false;
    }
}
