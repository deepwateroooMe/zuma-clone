using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 鑫龙口中吐出的小球带有这种脚本：每个小球自带的碰撞检测。只有鑫龙口中吐出的小球带有这种脚本，链表里的带有可碰撞的 Colliders
public class BallCollider : MonoBehaviour {

    private MoveBalls moveBallsScript;
    private bool onceFlag;

    void Start() {
        onceFlag = true;
        moveBallsScript = GameObject.FindObjectOfType<MoveBalls>();
    }
    void OnCollisionEnter(Collision other) {
        // 链表里的所有类型的小球：预设里全部标注为 ActiveBalls. 那么新发射的标注为 NewBall 的与链表里的 ActiveBalls 相碰撞，就能够检测到触发调用 
        if (other.gameObject.tag == "ActiveBalls" && onceFlag) { // 发生了有效碰撞：
            onceFlag = false;
            this.GetComponent<Rigidbody>().velocity = Vector2.zero; // 速度，即时清零
            this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition; // 冻结位置，凝固不动了
            this.gameObject.tag = "ActiveBalls"; // 将其变换为链表里同样的标签，方便将来它被碰撞检测 
            this.gameObject.layer = LayerMask.NameToLayer("ActiveBalls");
            // Get a vector from the center of the collided ball to the contact point
            ContactPoint contact = other.contacts[0]; // 它说，这种用法会造成GC, 性能低下
            Vector3 CollisionDir = contact.point - other.transform.position; // 碰撞方向失量
            int currentIdx = other.transform.GetSiblingIndex(); // 获取对象在Hierarchy面板的索引值（可以用于动态修改UGUI中UI的显示效果）
            float angle  = Vector3.Angle(CollisionDir, other.transform.forward); // 与发生碰撞的链条里小球的碰撞角度
            if ( angle > 90) // 从后面撞向前面小球前进的方向，加在被碰撞小球的后面
                moveBallsScript.AddNewBallAt(this.gameObject, currentIdx + 1, currentIdx);
            else // 从前面将被碰撞链条里的小球往前进反方向撞，新发射进来的小球取代被碰撞小球的位置，并将被碰小球往后推一位
                moveBallsScript.AddNewBallAt(this.gameObject, currentIdx, currentIdx);
            this.gameObject.GetComponent<BallCollider>().enabled = false; // 失活当前脚本
        }
    }
}