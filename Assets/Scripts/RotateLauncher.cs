using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这个类功能：负责管理，调控鑫龙头的转向，与发射小球，生成新的小球
public class RotateLauncher : MonoBehaviour {
    // 鑫龙嘴巴里的球：生成材料 
    public GameObject dummyBall; // 这是它的 Default 模型：用这个模子来生成其它嘴巴里的小球，但是它使用了RedBall Red-Material, 这是不对的
    public Material red;
    public Material blue;
    public Material green;
    public Material yellow;
    public Material stone;
    public Material white;
    public Material purple;
    public Material bonus;
    public static GameObject instanceBall; // 这个特殊的发射球：全局只有一个，任何时候都应该只有一个
    public float ballSpeed = 10;
    private Vector3 lookPos;

    private void Start() {
        CreateBall();
    }
    private void Update () {
        RotatePlayerAlongMousePosition(); // 把鑫龙的头，对准鼠标所在的方向 
        SetBallPostion(); // 每桢更新链表里球儿的位置
    }
    private void FixedUpdate() {
        ShootBall(); // TODO: 这里发射球：这可能是有时两粒，有时多粒的原因？WHY ？ 不该是只在用户点击的时候才发射球的吗？
    }
    // Rotate the launcher along the mouse position
    private void RotatePlayerAlongMousePosition () {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Camera.main.transform.position.y))
            lookPos = hit.point;
        Vector3 lookDir = lookPos - transform.position;
        lookDir.y = 0;
        transform.LookAt (transform.position + lookDir, Vector3.up); // 把金龙的头转向这个方向 
    }
    private void SetBallPostion() { // 这里更改的是：发射出去，在路上的小球的每桢的位置变化
        instanceBall.transform.forward = transform.forward;
        instanceBall.transform.position = transform.position + transform.forward * transform.localScale.z;
    }
    // TODO: 这里有几个 bug: FixedUpdate() 的时间与鼠标左键点下，是两个独立的事件，并不是在 FixedUpdate() 的这个时间点，正好发生鼠标左键点击。所以合理的做法应该是，每当鼠标点击，就将球发送出去？
    private void ShootBall() {
        // bool doneInThisFixedUpdate = false; // 目的是：想要一桢只最多发射一粒小球，为的是防卡死。现在是有太多每桢两三个小球会卡死的情况
        // if (Input.GetKeyDown(KeyCode.Mouse0) && !doneInThisFixedUpdate) { // 判断条件：并非每桢都自动生成，还是鼠标左键点击后金龙才会发射小球 [GetKeyDown：按下某个按键，按住只会在第一帧返回]
        if (Input.GetKeyDown(KeyCode.Mouse0)) { // 判断条件：并非每桢都自动生成，还是鼠标左键点击后金龙才会发射小球 [GetKeyDown：按下某个按键，按住只会在第一帧返回]
            // doneInThisFixedUpdate = true;
        // if (Input.GetMouseButtonDown(0)) { // 判断条件：并非每桢都自动生成，还是鼠标左键点击后金龙才会发射小球 [GetKeyDown：按下某个按键，按住只会在第一帧返回]
// 这里给发射出去的小球一个力度：因为力的作用，可以把小球链表往回打【会有可能三个效果：静止不支，向前或是向后！！】
// 效果上，如果能够把小球链表打得灰飞烟灭般动态多样，还是比较好玩的，【重温童年经典】
            instanceBall.GetComponent<Rigidbody>().AddForce(instanceBall.transform.forward * ballSpeed); 
            CreateBall(); // 发射出去一粒，这里就再生成一粒
        }
    }
    private void CreateBall() {
        instanceBall = Instantiate(dummyBall, transform.position, Quaternion.identity);
        instanceBall.SetActive(true);
        instanceBall.tag = "NewBall";
        instanceBall.gameObject.layer = LayerMask.NameToLayer("Default");
        SetBallColor(instanceBall);
    }
    private void SetBallColor(GameObject go) {
        BallColor randColor = MoveBalls.GetRandomBallColor();
        switch (randColor) { 
            case BallColor.red:
                go.GetComponent<Renderer>().material = red;
                break;
            case BallColor.green:
                go.GetComponent<Renderer>().material = green;
                break;
            case BallColor.blue: 
                go.GetComponent<Renderer>().material = blue;
                break;
            case BallColor.yellow:
                go.GetComponent<Renderer>().material = yellow;
                break;
            case BallColor.purple:
                go.GetComponent<Renderer>().material = purple;
                break;
            case BallColor.stone:
                go.GetComponent<Renderer>().material = stone;
                break;
            case BallColor.bonus:
                go.GetComponent<Renderer>().material = bonus;
                break;
            case BallColor.white:
                go.GetComponent<Renderer>().material = white;
                break;
        } 
    }
}