using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using DG.Tweening;
// 现在游戏中：这个太短了，原本只有8 个小球，现设置为 227 粒小球，需要有一个硬性个数设定，对不同关卡可以设置不同
public struct ActiveBallList {
    List<GameObject> ballList;
    bool isMoving;
    bool isInTransition;
}
public enum BallColor { // 太少，共8 种：这里强行添加多样性，8 种全加上
    red,
    green,
    blue,
    yellow,
    bonus,
    stone,
    purple,
    white
}
public class MoveBalls : MonoBehaviour {
    public GameObject redBall; // 这种设计不科学
    public GameObject greenBall;
    public GameObject blueBall;
    public GameObject yellowBall;
    public GameObject whiteBall;
    public GameObject bonusBall;
    public GameObject stoneBall;
    public GameObject purpleBall;
    public float pathSpeed; // 链条里的小球在轨道上的前进移动速度：我设置成了 9: 要速度快一点儿才好玩
    public float mergeSpeed;
    public int ballCount; // 227 粒小球
    public Ease easeType;      // 线性 Linear
    public Ease mergeEaseType; // 线性 Linear
    private List<GameObject> ballList;
    private GameObject ballsContainerGO;      // 它有个：活的小珠，存储器
    private GameObject removedBallsContainer; // 也有个：死的回收了的，存储器，但功能不全，没回收全？还是已经被再利用了？
    private BGCurve bgCurve;
    private float distance = 0;
    private int headballIndex;
    private SectionData sectionData;
    [SerializeField]
    private int addBallIndex;
    private int touchedBallIndex;
    private float ballRadius;
    private void Start () {
        ballRadius = redBall.transform.localScale.x;// 讨厌这种用法：设计狠不科学。把它调大了一点儿
        headballIndex = 0;
        addBallIndex = -1;
        bgCurve = GetComponent<BGCurve>();
        ballList = new List<GameObject>();
        ballsContainerGO = new GameObject();
        ballsContainerGO.name = "Balls Container";
        removedBallsContainer = new GameObject();
        removedBallsContainer.name = "Removed Balls Container";
        for (int i = 0; i < ballCount; i++)
            CreateNewBall();
        sectionData = new SectionData();
    }
    private void Update () {
        if (sectionData.ballSections.Count > 1 && addBallIndex != -1 && addBallIndex < headballIndex)
            MoveStopeedBallsAlongPath();
        if (ballList.Count > 0) // 最开始：只有一个片段
            MoveAllBallsAlongPath();
        if (headballIndex != 0) // 新发射球：加到了头上，晚点儿再看
            JoinStoppedSections(headballIndex, headballIndex - 1);
        if (addBallIndex != -1) // <<<<<<<<<<<<<<<<<<<< 新添加了一粒发射过来的球：游戏逻辑设置：想一下，消除一个片段后，想要把前面头部前进方向的片段往回拉吗？
            AddNewBallAndDeleteMatched();
        if (CheckIfActiveEndsMatch())
            MergeActiveEnds();
        MergeIfStoppedEndsMatch();
    }
    public void AddNewBallAt(GameObject go, int index, int touchedBallIdx) {
        addBallIndex = index; // addBallIndex != -1  // <<<<<<<<<<<<<<<<<<<< 
        touchedBallIndex = touchedBallIdx;
        if (index > ballList.Count) // 直接加到链表的尾巴上去【加进链条里统一管理】
            ballList.Add(go); 
        else // 插入到相应的位置 
            ballList.Insert(index, go); 
        go.transform.parent = ballsContainerGO.transform; // 设置父控件: 链条小球容器里
        go.transform.SetSiblingIndex(index); // 排位，标注正确下标
        if (touchedBallIdx < headballIndex) // 如果加在链条头，它成为了带头大哥，原头就后移。【那这里为什么不设置新的头呢？】
            headballIndex++;
        sectionData.OnAddModifySections(touchedBallIdx); // 被碰撞到的小球的下标
        // adjust distance for headBall or the position of the front in the added section
        PushSectionForwardByUnit(); // 这里就是，碰撞里产生的力的效果，添加一点儿游戏乐趣体验
    }
    private void InstatiateBall(GameObject ballGameObject) {
        GameObject go = Instantiate(ballGameObject,  bgCurve[0].PositionWorld, Quaternion.identity, ballsContainerGO.transform);
        go.SetActive(false);
        ballList.Add(go.gameObject);
    }
    // When a new Ball is added to the one of the stopped sections move the balls to their correct positions
    private void MoveStopeedBallsAlongPath() {
        int sectionKey = sectionData.GetSectionKey(addBallIndex);
        int sectionKeyVal = sectionData.ballSections[sectionKey];
        int movingBallCount = 1;
        float sectionHeadDist;
        GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[sectionKeyVal].transform.position, out sectionHeadDist);
        Vector3 tangent;
        Vector3 trailPos;
        float currentBallDist ;
        for (int i = sectionKeyVal + 1; i <= sectionKey; i++) {
            currentBallDist = sectionHeadDist - movingBallCount * ballRadius;
            trailPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(currentBallDist , out tangent);
            if (i == addBallIndex && addBallIndex != -1)
                ballList[i].transform.DOMove(trailPos, 0.5f).SetEase(easeType);
            else
                ballList[i].transform.DOMove(trailPos, 1);
            movingBallCount++;
        }
    }
    // Move the active section of balls along the path
    private void MoveAllBallsAlongPath() { // 两步逻辑：带头大哥第一个小球的移动；链条里其它 226 粒小球的移动；过程中考虑是否发射添加进来一粒小球
        Vector3 tangent;
        int movingBallCount = 1;
        distance += pathSpeed * Time.deltaTime; // 计算：单位时间内小球向前移动的增量
        // use a head index value which leads the balls on the path
        // This value will be changed when balls are delected from the path
        Vector3 headPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(distance, out tangent);
        ballList[headballIndex].transform.DOMove(headPos, 1); // 链条头：第一个小球 1 秒内移动到目标位置 headPos
        ballList[headballIndex].transform.rotation = Quaternion.LookRotation(tangent); // 目标旋转角度 
        if (!ballList[headballIndex].activeSelf) // 什么情况下会出现这种情况？
            ballList[headballIndex].SetActive(true);
        for (int i = headballIndex + 1; i < ballList.Count; i++) { // 带头小球 1 秒内设置到目标位置角度；链条里其它小球的处理 
            float currentBallDist = distance - movingBallCount * ballRadius; // 为什么这种情况下，需要考虑小球的半径？
            Vector3 trailPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(currentBallDist , out tangent);
            if (i == addBallIndex && addBallIndex != -1) // 如果这个位置新增了一个发射过来的小球
                ballList[i].transform.DOMove(trailPos, 0.5f).SetEase(easeType); // 0.5 秒移动到位：作线性运往到位。比普通运行快一倍
            else
                ballList[i].transform.DOMove(trailPos, 1); // 其它平移情况：慢一点儿，正常速度
            ballList[i].transform.rotation = Quaternion.LookRotation(tangent);
            if (!ballList[i].activeSelf) // 这里的情况是：没有出土的大概是失活的；等它需要出土了，它就需要被激活
                ballList[i].SetActive(true);
            movingBallCount++;
        }
    }
    // When using a different tween speed, there has to be a reset to the tween speed when the added balls reaches it correct position
    private bool CheckIfPushNeeded() {
        if (addBallIndex != ballList.Count) {
            Vector3 ABallPos = ballList[addBallIndex].transform.position;
            int neighbourBall = addBallIndex == touchedBallIndex? addBallIndex + 1: addBallIndex - 1;
            Vector3 TBallPos = ballList[neighbourBall].transform.position;
            if (Vector3.Distance(ABallPos, ballList[addBallIndex + 1].transform.position) <= 3)
                return true;
        }
        return false;
    }
// Move the section head a unit forward along the path when new ball is added
// 这里应该有可能的三种状态：静止不动，向前，向后，因为碰撞飞来的发射来的小球的力的作用方向是可能导致三种不同结果的
    private void PushSectionForwardByUnit() { // 这里只是把区段的头：向前1 秒内移动一个单位；后面的小球呢？如何处理？
        int sectionKey = sectionData.GetSectionKey(addBallIndex);
        int sectionKeyVal = sectionData.ballSections[sectionKey];
        float modifiedDistance;
        Vector3 tangent;
        GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[sectionKeyVal].transform.position, out modifiedDistance);
        modifiedDistance += ballRadius; // 同时，算上小球的半径
        if (addBallIndex >= headballIndex) // 加在头的后面
            distance = modifiedDistance;
        // 下面：小球的目标位置：
        Vector3 movedPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(modifiedDistance, out tangent);
        ballList[sectionKeyVal].transform.DOMove(movedPos, 1); // 这里说，同类型区段的头，1 秒内，移动到目标位置
        // TODO: 这里理解的逻辑还没能跟上：当同类型区片的头1 秒内移动到目标位置，每桢同步的时候，区段里其它小球是如何跟上的？
        ballList[sectionKeyVal].transform.rotation = Quaternion.LookRotation(tangent); // 同时，游戏效果，给它一定的计算出来的旋转度，看起来会比较好玩儿一点儿
    }
    // Join the sections which were divided when balls were removed
    // Just check the current head with the next value if they are close
    private void JoinStoppedSections(int currentIdx, int nextSectionIdx) {
        float nextSecdist;
        GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[nextSectionIdx].transform.position, out nextSecdist);
        if (nextSecdist - distance <= ballRadius) {
            int nextSectionKeyVal;
            sectionData.ballSections.TryGetValue(nextSectionIdx, out nextSectionKeyVal);
            headballIndex = nextSectionKeyVal; // 这里更新成：前进方向，前一个片段的头，未必是同类型的
            MergeSections(currentIdx, nextSectionKeyVal);
            RemoveMatchedBalls(nextSectionIdx, ballList[nextSectionIdx]);
            if (ballList.Count > 0) {
                GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[headballIndex].transform.position, out nextSecdist);
                distance = nextSecdist;
            }
        }
    }
    private void MergeSections(int currentIdx, int nextSectionKeyVal) {
        sectionData.ballSections.Remove(currentIdx - 1);
        sectionData.ballSections[int.MaxValue] = nextSectionKeyVal;
    }
    // Check if the added new ball has reached its correct position on the path along the curve
    // Also remove the match ball upon reaching the position
    // set a flag to know if the ball has reached its correct position
    private void AddNewBallAndDeleteMatched() {
        // check if the ball position is on the path
        int sectionKey = sectionData.GetSectionKey(addBallIndex);
        int sectionKeyVal = sectionData.ballSections[sectionKey];
        float neighbourDist = 0;
        Vector3 currentTangent;
        Vector3 actualPos = Vector3.zero;
        Vector3 neighbourPos = Vector3.zero;
        int end = sectionKey == int.MaxValue? ballList.Count - 1: sectionKey; // 区段的尾巴下标：链条长度【最开始的一个区段】，或更短
        if (addBallIndex == end) { // 如果加在链条最尾巴上，刚出来的刚可看见的地方
            neighbourPos = ballList[addBallIndex - 1].transform.position; 
            GetComponent<BGCcMath>().CalcPositionByClosestPoint(neighbourPos, out neighbourDist); // 什么情况下需要考虑半径，什么情况下不需要，还没想狠清楚 
            actualPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(neighbourDist - ballRadius, out currentTangent);
        } else {
            neighbourPos = ballList[addBallIndex + 1].transform.position; // 链条：当前插入位置后面的邻居 
            GetComponent<BGCcMath>().CalcPositionByClosestPoint(neighbourPos, out neighbourDist);
            actualPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(neighbourDist + ballRadius, out currentTangent);
        }
        Vector2 currentPos = new Vector2(ballList[addBallIndex].transform.position.x, ballList[addBallIndex].transform.position.y);
        float isNear = Vector2.Distance(actualPos, currentPos);
// TODO: 这个限制条件是让我想不明白的：因为游戏过程中存在，同类型好几个消除不了的情况? 后来好像看见得少了，可是仍然需要再检测一下
        if (isNear <= 0.1f) {  // 想把这个限制条件去掉
            RemoveMatchedBalls(addBallIndex, ballList[addBallIndex]);
            addBallIndex = -1;
        }
    }
    // Called by the collided new ball on collision with the active balls on the path
    private void RemoveMatchedBalls(int index, GameObject go) {
        int front = index;
        int back = index;
// TODO: 这里就是增加了类型之后，逻辑没有连通的地方：8 种类型，目前以不同的材质相区分，而不是使用颜色
        string curInsertedMaterial = go.GetComponent<Renderer>().material.name.Substring(0, 3);
        int sectionKey = sectionData.GetSectionKey(index);
        int sectionKeyVal;
        sectionData.ballSections.TryGetValue(sectionKey, out sectionKeyVal);
        // Check if any same color balls towards the front side
        string currentMaterial = null;
        for (int i = index - 1; i >= sectionKeyVal; i--) { // 往链条头的方向遍历：查找相同材质的小球片段
            currentMaterial = ballList[i].GetComponent<Renderer>().material.name.Substring(0, 3);
            if (currentMaterial == curInsertedMaterial)
                front = i; // 这里会继续往前遍历，找出同类型片段 
            else // 是不同类型，中断查找
                break;
        }
        // Check if any same color balls towards the back side
        int end  = sectionKey == int.MaxValue ? ballList.Count - 1: sectionKey;
        for (int i = index + 1; i <= end ; i++) {
            currentMaterial = ballList[i].GetComponent<Renderer>().material.name.Substring(0, 3);
            if (currentMaterial == curInsertedMaterial)
                back = i;
            else
                break;
        }
        // If atleast 3 balls in a row are found at the new balls position
        if (back - front >= 2) { // 同类型片段，至少 3 个
            // Modify the headballIndex only if the remove section is in the moving section
            if (back > headballIndex) {
                // whole back section will be removed change the headIndex to the front section value
                if (front == sectionKeyVal && back == ballList.Count - 1) { // 这整个片段会被消除
                    if (sectionData.ballSections.Count > 1) {
                        int nextSectionFront;
                        sectionData.ballSections.TryGetValue(front - 1, out nextSectionFront);
                        headballIndex = nextSectionFront; // 这里倒着往前遍历更新 headballIndex 的值，成为前一个片段的头
                    }
                }
                // if the remove section is less that the back i.e front and middle part of the moving section
                else { // 当前片段的子片段 ? 会被消除 
                    if (front >= sectionKeyVal && back != ballList.Count - 1) {
                        headballIndex = front; // 当前最小同类型片段的头，设置 
                    }
                }
            } else {
                headballIndex -= (back - front + 1); // 把头，移到当前同类型片段的前面去
            }
            Debug.Log("HEAD INDEX:" + headballIndex);
            RemoveBalls(front, back - front + 1);
            if (back > headballIndex && ballList.Count > 0)
                GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[headballIndex].transform.position, out distance);
        }
    }
    // Remove balls from the list also from the ballsContainer in scene
    private void RemoveBalls(int atIndex, int range) {
        for (int i = 0; i < range; i++) {
            ballList[atIndex + i].transform.parent = removedBallsContainer.transform; // 重新设置父控件：回收池父控件 
            ballList[atIndex + i].SetActive(false); // 失活
        }
        ballList.RemoveRange(atIndex, range); // 删除链表节点及引用
        OnDeleteModifySections(atIndex, range);
    }
    private void OnDeleteModifySections(int atIndex, int range) {
        int sectionKey = sectionData.GetSectionKey(atIndex);
        int sectionKeyVal;
        sectionData.ballSections.TryGetValue(sectionKey, out sectionKeyVal);
        // completely remove the section
        if (atIndex == sectionKeyVal && atIndex + range == ballList.Count + range) {
            sectionData.DeleteEntireSection(atIndex, range, sectionKey, ballList.Count);
        }
        else {
            sectionData.DeletePartialSection(atIndex, range, sectionKey, sectionKeyVal, ballList.Count);
        }
    }
    // Check if the active section front ball matches with the back of the next section
    private bool CheckIfActiveEndsMatch() {
        if (sectionData.ballSections.Count <= 1)
            return false;
        string headBallMaterial = ballList[headballIndex].GetComponent<Renderer>().material.name.Substring(0, 3);
        string nextSectionEndMaterial = ballList[headballIndex - 1].GetComponent<Renderer>().material.name.Substring(0, 3);
        if (headBallMaterial == nextSectionEndMaterial)
            return true;
        return false;
    }

    // Move the section to the front of the active section
    private void MergeActiveEnds () {
        int sectionKey = headballIndex - 1;
        int sectionKeyVal = sectionData.ballSections[sectionKey];
        int movingBallCount = 1;
        float sectionHeadDist;
        GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[sectionKey].transform.position, out sectionHeadDist);
        sectionHeadDist -= mergeSpeed * Time.deltaTime; // 单位时间内的，合并距离 
        Vector3 tangent;
        Vector3 trailPos =  GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(sectionHeadDist, out tangent);
        ballList[sectionKey].transform.DOMove(trailPos, 0.3f).SetEase(mergeEaseType); // 0.3 秒之内，移动到位，线性运动【头部：前一个片段最后一个球，回缩运动】
        ballList[sectionKey].transform.rotation = Quaternion.LookRotation(tangent); // 这里为什么没有？我把它加上了？！！！
        for (int i = sectionKey - 1; i >= sectionKeyVal; i--) { // 前一个片段：从倒数第二个到第一个，往后回缩运行
            float currentBallDist = sectionHeadDist + movingBallCount * ballRadius;
            trailPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(currentBallDist , out tangent);
            ballList[i].transform.DOMove(trailPos, 0.3f).SetEase(easeType);
            ballList[i].transform.rotation = Quaternion.LookRotation(tangent);
            movingBallCount++;
        }
    }
    // Merge the Stopped End balls
    // 这里是什么意思呢？没有实现，还是没有必要实现？多打几次，再想一想。
    private void MergeIfStoppedEndsMatch() {
        // 【爱表哥，爱生活！！！活宝妹就是一定要嫁给亲爱的表哥！！！】        
    }

    public static BallColor GetRandomBallColor() {
        int rInt = Random.Range(0, 8); // 这个生成少了，黄色的就出不来
        return (BallColor)rInt;
    }
    private void CreateNewBall() {
        switch (GetRandomBallColor()) {
        case BallColor.red:
            InstatiateBall(redBall);
            break;
        case BallColor.green:
            InstatiateBall(greenBall);
            break;
        case BallColor.blue:
            InstatiateBall(blueBall);
            break;
        case BallColor.yellow:
            InstatiateBall(yellowBall);
            break;
        case BallColor.bonus:
            InstatiateBall(bonusBall);
            break;
        case BallColor.stone:
            InstatiateBall(stoneBall);
            break;
        case BallColor.purple:
            InstatiateBall(purpleBall);
            break;
        case BallColor.white:
            InstatiateBall(whiteBall);
            break;
        }
    }
}

