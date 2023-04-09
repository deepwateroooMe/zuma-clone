using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
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
        ShootBall(); 
    }
    private void RotatePlayerAlongMousePosition () { // Rotate the launcher along the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Camera.main.transform.position.y))
            lookPos = hit.point;
        // Vector3 lookDir = lookPos - transform.position;
        // lookDir.y = 0;
        // transform.LookAt(transform.position + lookDir, Vector3.up);  // 这里是向上看吗？不该是往前看才对吗？

        // 1.得到当前位置和目标位置的方向
        Vector3 dir = (new Vector3(lookPos.x, 0, lookPos.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
        // 2.的到的方向通过LookRotation函数获得这个方向的四元数
        Quaternion rot = Quaternion.LookRotation(dir);
        // 3.使用插件DOTween中DORotate的旋转得到角度
        transform.DORotate(new Vector3(rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z), 0.01f, RotateMode.Fast);
    }
    private void SetBallPostion() { // 这里更改的是：发射出去，在路上的小球的每桢的位置变化
        instanceBall.transform.forward = transform.forward;
        // instanceBall.transform.position = transform.position;
        instanceBall.transform.position = transform.position + transform.forward * transform.localScale.z;//ori
        // 把下面的去掉，它就似乎又可以正常碰撞检测了。感觉可以理解无法碰撞到是因为往天上发射，但没明白下面的【位置向前移动一点儿，向下移动一点儿，已经改变了小球的相对发射方向】
        // instanceBall.transform.position = transform.position + transform.forward * 4;  // 改成这个样子的就不可以
        // instanceBall.transform.position = transform.position - new Vector3(0f, 2f, 0f); // 这里只是把发射小球的高度降低 
        // instanceBall.transform.position = transform.position;
    }
    // 【爱表哥，爱生活！！！活宝妹就是一定要嫁给亲爱的表哥！！！】
    private void ShootBall() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) { // 判断条件：并非每桢都自动生成，还是鼠标左键点击后金龙才会发射小球 [GetKeyDown：按下某个按键，按住只会在第一帧返回]
// 这里给发射出去的小球一个力度：因为力的作用，可以把小球链表往回打【会有可能三个效果：静止不支，向前或是向后！！】
// 效果上，如果能够把小球链表打得灰飞烟灭般动态多样，还是比较好玩的，【重温童年经典】
            instanceBall.GetComponent<Rigidbody>().AddForce(instanceBall.transform.forward * ballSpeed); 
            CreateBall(); // 发射出去一粒，这里就再生成一粒
        }
    }
    private void CreateBall() { // TODO: 这里因为球的位置不对，需要给它一个相对偏移, 需要考虑青蛙的旋转。背上的球同理
        instanceBall = Instantiate(dummyBall, transform.position, Quaternion.identity); // 青蛙以它自己为中心旋转；但是两粒小球都有相对偏移
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