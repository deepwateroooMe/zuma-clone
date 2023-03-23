using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这个类功能：负责管理，调控鑫龙头的转向，与发射小球，生成新的小球
public class RotateLauncher : MonoBehaviour {
    public GameObject dummyBall; // 这是它的 Default 模型：用这个模子来生成其它嘴巴里的小球
    public float ballSpeed = 10;
    public GameObject instanceBall;
    private Vector3 lookPos;

    private void Start() {
        CreateBall();
    }
    // Update is called once per frame
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
    // Set balls postions and forward w.r.t to the launcher
    private void SetBallPostion() {
        instanceBall.transform.forward = transform.forward;
        instanceBall.transform.position = transform.position + transform.forward * transform.localScale.z;
    }
    private void ShootBall() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) { // 判断条件：并非每桢都自动生成，还是鼠标左键点击后金龙才会发射小球
// 这里给发射出去的小球一个力度：因为力的作用，可以把小球链表往回打【会有可能三个效果：静止不支，向前或是向后！！】，还比较好玩儿。
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
// 现在明白了：鑫龙嘴巴里的球，的生成方法与，链表里的球的生成方法不一样
    // 链表里球：有各种不同类型的预设，直接根据预设来生成实例，使用到的是不同类型的Materail
    // 这里鑫龙嘴巴里的球：只用到了一个预设，RedBall, 它使用的材质是 Red-Material, 然后这里的逻辑，给它身上强加了几种不同的颜色，这是不对的，当然与链表里的球不同一样
    // TODO: 这里生成小球的方法：将使用的模块 RedBall 的Red-Material 去掉，使用无材质预设；然后这里，根据颜色，给他们加上相应的材质，而非在红材质身上强加颜色，就会对了
    // 同样，因为使用了不同材质，明白这了这里産不同的原理，就可以把那些不必要的颜色全部去掉【亲爱的表哥，活宝妹一定要嫁的亲爱的表哥！！！活宝妹就是一定要嫁给亲爱的表哥！！！】
        switch (randColor) { 
            case BallColor.red:
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                break;
            case BallColor.green:
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                break;
            case BallColor.blue: // TODO: 这里一个 bug: 为什么我蓝色的球儿，有的色深，有的色浅？
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
                break;
            case BallColor.yellow:
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
                break;
        }
    }
    // private void SetRandomColor(GameObject go) { // 这里随机生成的颜色：与系统不符合，去调所有调用这里的地方
    //     Color color = new Color(Random.Range(0F,1F), Random.Range(0, 1F), Random.Range(0, 1F));
    //     go.GetComponent<Renderer>().material.SetColor("_Color", color);
    // }
}