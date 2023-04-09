using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 这个碰撞器：放在洞口，每撞一个小球，就回收这个小球；接第一个的时候就说，游戏结束。结束的标志是，链条上的小球的速度飞快，是以前的比如说 5 倍速度前进
public class GameEndCollider : MonoBehaviour {
    private const string TAG = "GameEndCollider";
    private MoveBalls moveBallsScript;
    // private bool onceFlag;
    private bool isGameOver = false;
    private GameObject otherGO;
    void Start() {
        // onceFlag = true;
        moveBallsScript = GameObject.FindObjectOfType<MoveBalls>();
    }
    // 这里没有写好：能够检测到碰撞，游戏结束能够提速，可是没有销毁小球
    void OnCollisionEnter(Collision other) {
        // 链表里的所有类型的小球：预设里全部标注为 ActiveBalls. 那么新发射的标注为 NewBall 的与链表里的 ActiveBalls 相碰撞，就能够检测到触发调用
        if (!isGameOver) {
            isGameOver = true;
            moveBallsScript.pathSpeed *= 5; // 以前链条上滚动速度的 5 倍
        }
        if (other.gameObject != null && other.gameObject.tag == "ActiveBalls") { // 发生了有效碰撞：但是根据打印出来的日志，这里怎么是检测出了无数次呢
            Debug.Log(TAG + " OnCollisionEnter() 真正检测到碰撞"); // 能够打印很多这个日志
            Debug.Log(TAG + " other.gameObject.name: " + other.gameObject.name);
            otherGO = other.gameObject;
            // 这里暂时就把这个小球直接消毁掉吧：应该是使用对象池来回收的
            // 下面的做法，仍然是简单粗暴，把它销毁了，但是链条里的数据结构的管理没有跟上：比如头的下标位置，需要转移到下一个等等
            // GameObject.Destroy(other.gameObject);
            other.gameObject.SetActive(false); // 不知道为什么这个就没有失活掉？？？是因为程序里把它又激活了；要去找轨道的第一个点【接近洞口的那个点】，在那里停止和销毁 
        }
    }
    // void OnCollisionExit(Collision other) { // 这样是不对的，检测不到这个方法
    //     Debug.Log(TAG + " OnCollisionExit()");
    //     if (!isGameOver) {
    //         isGameOver = true;
    //         moveBallsScript.pathSpeed *= 5; // 以前链条上滚动速度的 5 倍
    //     }
    //     if (otherGO != null && otherGO.activeSelf) {
    //         Debug.Log(TAG + " otherGO.name: " + otherGO.name);
    //         otherGO.SetActive(false); // 不知道为什么这个就没有失活掉？？？
    //     }
    // }
}