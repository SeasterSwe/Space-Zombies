using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragToShoot : MonoBehaviour
{
    public float power = 5;

    public Vector2 minPower;
    public Vector2 maxPower;

    Vector2 force;
    Vector2 startPos;
    Vector2 endPos;

    bool refuel;

    Camera cam;
    Rigidbody2D rb;
    TracjectoryLine tl;
    Fuel fuel;
    public Transform rotationTarget;
    public LayerMask ignoreMask;
    public PlayerDmgTrail dmgTrail;
    public LineRenderer aimLine; Animator animator;

    private void Awake()
    {
        animator = transform.Find("main_shootoff").gameObject.GetComponent<Animator>();
        transform.Find("Jetpack").gameObject.GetComponent<PlayerDmgTrail>();
        aimLine = transform.Find("Jetpack").gameObject.GetComponent<LineRenderer>();
        aimLine.positionCount = 2;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        cam = Camera.main;
        tl = GetComponent<TracjectoryLine>();
        fuel = GetComponentInChildren<Fuel>();
        //p = transform.Find("Jetpack").transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    //Different fuel usage on how far you drag arrow?!?
    void Update()
    {
        if (!(Time.timeScale > 0))
            return;

        DragAndShoot();

        if (!Input.GetMouseButton(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 3f, ~ignoreMask);
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                Vector2 center = hit.point - (Vector2)transform.position;
                var angle = Mathf.Atan2(center.y, center.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
            }
        }
    }

    Vector2 halfVel;
    void DragAndShoot()
    {
        if (fuel.GetCurrentFuel() > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startPos = cam.ScreenToWorldPoint(Input.mousePosition);
                halfVel = rb.velocity.normalized * 0.3f * power;
                dmgTrail.canDmg = false;
                aimLine.enabled = true;
                animator.SetTrigger("Aim");
            }

            if (Input.GetMouseButton(0))
            {
                dmgTrail.aiming = true;
                Vector2 currentPoint = cam.ScreenToWorldPoint(Input.mousePosition);
                tl.RenderLine(startPos, currentPoint);
                rb.velocity = halfVel;

                Vector2 center = currentPoint - startPos;
                var angle = Mathf.Atan2(center.y, center.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
                aimLine.SetPosition(0, transform.position);
                aimLine.SetPosition(1, transform.position + transform.TransformDirection(Vector3.up * 2));
            }

            if (Input.GetMouseButtonUp(0))
            {
                AudioManager.instace.Play("Land", transform);
                aimLine.enabled = false;
                dmgTrail.canDmg = true;
                dmgTrail.aiming = false;
                dmgTrail.LaunchEffekt();
                endPos = cam.ScreenToWorldPoint(Input.mousePosition);
                rb.velocity = Vector2.zero;

                float forceMutli = Vector2.Distance(startPos, endPos);
                forceMutli = Mathf.Clamp(forceMutli, 0, 4);
                force = new Vector2(Mathf.Clamp(startPos.x - endPos.x, minPower.x, maxPower.x), Mathf.Clamp(startPos.y - endPos.y, minPower.y, maxPower.y));
                rb.AddForce(force.normalized * power * forceMutli, ForceMode2D.Impulse);

                fuel.UseFuel(10);
                tl.DisableLine();
                animator.SetTrigger("RealeseAim");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            rb.velocity = Vector2.zero;
            halfVel = Vector2.zero;
            dmgTrail.canDmg = false;
            dmgTrail.aiming = false;
            AudioManager.instace.Play("Launch", transform);
            animator.SetTrigger("Land");
        }
    }

    float timeToNextFuel = 0;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            if (timeToNextFuel <= Time.time)
            {
                timeToNextFuel = Time.time + 1;
                fuel.AddFule(5);
            }
        }
    }
}
