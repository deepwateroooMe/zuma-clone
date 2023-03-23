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
        if (other.gameObject.tag == "ActiveBalls" && onceFlag) {
            onceFlag = false;
            this.GetComponent<Rigidbody>().velocity = Vector2.zero;
            this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            this.gameObject.tag = "ActiveBalls"; // 将其变换为链表里同样的标签，方便将来它被碰撞检测 
            this.gameObject.layer = LayerMask.NameToLayer("ActiveBalls");
            // Get a vector from the center of the collided ball to the contact point
            ContactPoint contact = other.contacts[0];
            Vector3 CollisionDir = contact.point - other.transform.position;
            int currentIdx = other.transform.GetSiblingIndex();
            float angle  = Vector3.Angle(CollisionDir, other.transform.forward);
            if ( angle > 90) // 这里添加的位置有一点点儿的不同
                moveBallsScript.AddNewBallAt(this.gameObject, currentIdx + 1, currentIdx);
            else
                moveBallsScript.AddNewBallAt(this.gameObject, currentIdx, currentIdx);
            this.gameObject.GetComponent<BallCollider>().enabled = false; // 失活当前脚本
        }
    }
}