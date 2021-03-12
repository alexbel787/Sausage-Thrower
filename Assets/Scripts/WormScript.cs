using System.Collections;
using UnityEngine;

public class WormScript : MonoBehaviour
{
    public float power;
    public float maxDrag = 5f;
    public Rigidbody rb;
    public LineRenderer lr;

    private Vector3 dragStartPos;
    private Vector3 touchPos;
    private Vector3 clampedForce;
    public bool grounded = true;
    private bool onGround;
    private bool doOnce;
    private bool showTrajectory;

    private GameManagerScript GMS;

    private void Start()
    {
        rb.sleepThreshold = 0;
        GMS = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    private void Update()
    {
        if (grounded)
        {
#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                touchPos = touch.position;
                touchPos.z = 10;

                if (touch.phase == TouchPhase.Began)
                {
                    DragStart();
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    DragRelease();
                }
            }
#endif

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                touchPos = Input.mousePosition;
                touchPos.z = 10;
                DragStart();
            }

            if (Input.GetMouseButton(0))
            {
                touchPos = Input.mousePosition;
                touchPos.z = 10;
            }

            if (Input.GetMouseButtonUp(0))
            {
                touchPos = Input.mousePosition;
                touchPos.z = 10;
                DragRelease();
            }
#endif

            if (showTrajectory) ShowTrajectory();
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Steps") && !SoundManager.instance.GetComponent<SoundManager>().soundSource.isPlaying)
        {
            GMS.ScoreText.text = "ALTITUDE " + (int)transform.position.y;
            if (rb.velocity.magnitude > .6f)
                SoundManager.instance.RandomizeSfx(.6f, SoundManager.instance.smackSounds);
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            SoundManager.instance.RandomizeSfx(1, SoundManager.instance.fallSounds);
            GMS.GameOver();
            this.enabled = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Steps"))
        {
            onGround = true;
            if (!doOnce)
            {
                doOnce = true;
                StartCoroutine(CheckOnGroundTimer());
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Steps"))
        {
            onGround = false; // If jumped, bouncing or rolling with no ground touch - then player can't jump
        }
    }

    private IEnumerator CheckOnGroundTimer()
    {
        yield return new WaitForSeconds(.5f);
        if (onGround) grounded = true;
        doOnce = false;
    }

    private void DragStart()
    {
        dragStartPos = Camera.main.ScreenToWorldPoint(touchPos);
        dragStartPos.z = 0;
        showTrajectory = true;
    }

    private void DragRelease()
    {
        grounded = false;
        showTrajectory = false;
        lr.positionCount = 0;
        rb.AddForce(clampedForce, ForceMode.Impulse);
        SoundManager.instance.RandomizeSfx(1, SoundManager.instance.jumpSounds);
    }

    private void ShowTrajectory()
    {
        Vector3 draggingPos = Camera.main.ScreenToWorldPoint(touchPos);
        draggingPos.z = 0;
        Vector3 force = dragStartPos - draggingPos;
        clampedForce = Vector3.ClampMagnitude(force, maxDrag) * power;
        print("force " + force + " / " + "clampedForce " + clampedForce);
        Vector3[] points = new Vector3[10];
        lr.positionCount = points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            float time = i * 0.1f;

            points[i] = transform.position + clampedForce / power * time + Physics.gravity * time * time / 2f;

            if (points[i].y < 0)
            {
                lr.positionCount = i + 1;
                break;
            }
        }

        lr.SetPositions(points);
    }

}
