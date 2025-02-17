﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 想个办法：打些日志，把这个类理解清楚一点儿，好修改项目里无数的 bug
public class SectionData {
    private const string TAG = "SectionData";
    // 没搞明白：这个字典是在纪录什么？分片的球，同色的起始下标？
    public SortedDictionary<int, int> ballSections; // 它的键值对：像是同样花色的片段，首尾的下标？【尾，头】

    public SectionData() {
        ballSections = new SortedDictionary<int, int>(); // 有序，升序字典
        ballSections.Add(int.MaxValue, 0); // [Integer.MAX_VALUE, 0] 【尾，头】 这里尾比头大
    }
    public int GetSectionKey(int front) {
        int key = int.MaxValue;
        foreach (KeyValuePair<int, int> entry in ballSections) {
            if (front >= entry.Value && front <= entry.Key) // 找到一个介于头尾之间的片段
                key = entry.Key;
        }
        return key;
    }

    public void OnAddModifySections(int atIndex) { // atIndex: 被碰撞小球的下标，可添加在其前，或是其后
        List<KeyValuePair<int, int>> modSectionList = new List<KeyValuePair<int, int>>();
        int sectionKey = GetSectionKey(atIndex); 
        int sectionKeyVal;
        ballSections.TryGetValue(sectionKey, out sectionKeyVal);
        if (sectionKey != int.MaxValue) { // 最开始拿的，都是最大值 Integer.MAX_VALUE, 什么时候才会变呢？
            Debug.Log(TAG + " OnAddModifySections() sectionKey: " + sectionKey);
            int newSectionKey = sectionKey + 1;
            ballSections.Add(newSectionKey, sectionKeyVal); // 头不变，尾变大了：因为新加入了一粒小球
            ballSections.Remove(sectionKey); // 移除之前的
            // 不懂它的设计原理：还是没有读懂，它下面的两个部分是在做什么，当一个小球插入移动链表，它需要更新些什么？
            // Get the keys which are to be updated
            // Since changing the value in the loop itself will add new entries which are always greater than the current one
            // There by looping it infinitely and incorrect
            foreach (KeyValuePair<int, int> entry in ballSections) {
                if (entry.Key > newSectionKey)
                    modSectionList.Add(entry);
            }
            // 打印修改过的片段
            foreach (KeyValuePair<int, int> entry in modSectionList) {
                Debug.Log(TAG + " entry.[Key, Value]: [" + entry.Key + ", " + entry.Value + "]");
            }
            // Update the sections seperatly to avoid wrong calulation when done in above loop
            // For all the value other than the end of chain modifiy both their key and value
            // For the end of chain section only update the value i.e its front
            foreach (KeyValuePair<int, int> entry in modSectionList) {
                if (entry.Key != int.MaxValue) {
                    if (entry.Value == 0)
                        ballSections.Add(entry.Key + 1, entry.Value);
                    else
                        ballSections.Add(entry.Key + 1, entry.Value + 1);
                    ballSections.Remove(entry.Key);
                } else
                    ballSections[entry.Key] = entry.Value + 1;
            }
        }
    }

    public void DeleteEntireSection(int atIndex, int range, int sectionKey, int ballListCount) {
        List<KeyValuePair<int, int>> modSectionList = new List<KeyValuePair<int, int>>();
        // If section is last but one i.e before the moving section
        if (atIndex + range != ballListCount + range) {
            Debug.Log("Entire: ");
            // modify the back values for the immediate next section.
            // modify the back and front values for other higher ones
            ballSections.Remove(sectionKey);
            foreach (KeyValuePair<int, int> entry in ballSections) {
                if (entry.Key > sectionKey)
                    modSectionList.Add(entry);
            }
            ballSections.Add(modSectionList[0].Key - range, atIndex); // 这里看昏了；没看懂
            ballSections.Remove(modSectionList[0].Key);
            modSectionList.RemoveAt(0);
            foreach(KeyValuePair<int, int> entry in modSectionList) {
                if (entry.Key != int.MaxValue) {
                    ballSections.Add(entry.Key - range, entry.Value - range);
                    ballSections.Remove(entry.Key);
                } else
                    ballSections[entry.Key] = entry.Value - range;
            }
        }
        else { // 把键为 sectionKey 的这个键值对去掉，保留最后一个大片段
            KeyValuePair<int, int> getLastButOne = new KeyValuePair<int, int>();
            if (ballSections.Count > 1) {
                foreach (KeyValuePair<int, int> entry in ballSections) {
                    if (entry.Key < sectionKey)
                        getLastButOne = entry;
                }
                ballSections.Remove(getLastButOne.Key);
                ballSections[int.MaxValue] = getLastButOne.Value;
            }
        }
    }
    public void DeletePartialSection(int atIndex, int range, int sectionKey, int sectionKeyVal, int ballListCount) {
        List<KeyValuePair<int, int>> modSectionList = new List<KeyValuePair<int, int>>();
        // Handle cases
        // when delection takes place at the front or back of the section
        int end = sectionKey == int.MaxValue? ballListCount + range - 1: sectionKey;
        if (atIndex == sectionKeyVal || atIndex + range - 1 == end) {
            Debug.Log("Partial: Front/back 是原？片段的头，或是尾巴部分");
            if (sectionKey == int.MaxValue)
                return;
            int newSectionKey = sectionKey - range; // 缩小尾巴下标
            ballSections.Add(newSectionKey, sectionKeyVal); // 更新成新片段
            ballSections.Remove(sectionKey); // 去除原大片段
            foreach (KeyValuePair<int, int> entry in ballSections) {
                if (entry.Key > newSectionKey)
                    modSectionList.Add(entry);
            }
        }
        // when delection takes place in middle of the section which creates a new section
        else {
            Debug.Log("Partial: Middle 是原 ? 片段的中间某部分");
            // new section front: 如果要去掉一个中间片段，就会产生一个原中间片段之前的前片段，和一个后片段
            int newSectionKey = atIndex - 1;
            ballSections.Add(newSectionKey, sectionKeyVal); // 添加一个新的：原中间片段之前的，前片段 
            if (sectionKey != int.MaxValue) {
                int nextSectionKey = sectionKey - range; // 因为当前大片段，只会消除 Range 个小球，所以它的尾巴下标会变短这么多
                ballSections.Remove(sectionKey);  // 去掉，原大片段 
                // new section back
                ballSections.Add(nextSectionKey, atIndex);  // 添加一个新的：原中间片段之后的，后片段
                foreach (KeyValuePair<int, int> entry in ballSections) {
                    if (entry.Key > nextSectionKey)
                        modSectionList.Add(entry);
                }
            } else { // 最后一个大片段的话，就重新更新一下头就可以了
                ballSections[int.MaxValue] = atIndex;
                return;
            }
        }
        // Sort keys just to avoid bug due if a lower key is modified before higher key
        modSectionList.Sort((entryA, entryB) => entryA.Key.CompareTo(entryB.Key)); // 这里都是计算了的，需要更新的缓存，还仍需要进一步地修改
        // Get the keys to be changed and modify them
        foreach(KeyValuePair<int, int> entry in modSectionList) { // 对受到影响，还没更改到位的缓存，再作进一步的修改，同步到位
            if (entry.Key != int.MaxValue) {
                if (entry.Value == 0)
                    ballSections.Add(entry.Key - range, entry.Value);
                else
                    ballSections.Add(entry.Key - range, entry.Value - range);
                ballSections.Remove(entry.Key);
            }
            else
                ballSections[entry.Key] = entry.Value - range;
        }
    }
}