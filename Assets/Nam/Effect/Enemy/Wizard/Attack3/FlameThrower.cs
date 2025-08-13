using UnityEngine;
using UnityEngine.VFX;

public class FlameThrower : MonoBehaviour
{

    public VisualEffect flameThrower;

    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private float rotateSpeed = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flameThrower.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            flameThrower.Play();
        }
        if (Input.GetMouseButtonUp(0))
        {
            flameThrower.Stop();
        }

        //w,s,a,d Å° ÀÔ·Â, arrow key    
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            this.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            this.transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            this.transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            this.transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            this.transform.Rotate(Vector3.up * -rotateSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            this.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        }
    }
}
