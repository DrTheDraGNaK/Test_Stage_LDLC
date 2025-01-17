using UnityEngine;

public class PaintGun : MonoBehaviour
{
    [SerializeField] private ColorType gunColor;
    [SerializeField] private GameObject paintBallPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootForce = 20f;
    [SerializeField] private float shootCooldown = 0.2f;

    private float lastShootTime;

    public ColorType GetGunColor()
    {
        return gunColor;
    }

    public void Shoot()
    {
        if (Time.time - lastShootTime < shootCooldown)
            return;

        GameObject paintBall = Instantiate(paintBallPrefab, shootPoint.position, shootPoint.rotation);
        PaintBall ball = paintBall.GetComponent<PaintBall>();
        ball.Initialize(gunColor);

        Rigidbody rb = paintBall.GetComponent<Rigidbody>();
        rb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);

        lastShootTime = Time.time;
    }
}
