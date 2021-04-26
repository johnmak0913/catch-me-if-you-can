﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Action : MonoBehaviour {
    MainControl control;
    PlayerAnimation pAnim;
    KeyCode actionKey;
    string clip;
    float holdFor, heldTime=0;
    float cd, cdLeft;
    float xOffset, yOffset;
    int marks;
    bool isCooling=false;
    bool finish=true;
    bool acting=false;
    Transform player;
    GameObject cdIcon;
    AudioSource audioSource;
    AudioClip audioClip;

    public Action(string clip, string audio, string cdIcon, KeyCode actionKey, float holdFor, float cd, int marks, float xOffest=0f, float yOffset=0f) {
        this.clip=clip;
        this.actionKey=actionKey;
        this.holdFor=holdFor;
        this.cd=cd;
        this.marks=marks;
        this.xOffset=xOffest;
        this.yOffset=yOffset;
        this.cdIcon=GameObject.Find("UI").transform.Find(cdIcon).gameObject;  // NullReferenceException??
        this.cdIcon.SetActive(false);
        audioSource=GameObject.Find("Player").GetComponent<AudioSource>();
        audioClip=Resources.Load<AudioClip>("Audio/"+audio);
        pAnim=GameObject.Find("Player").GetComponent<PlayerAnimation>();
        control=GameObject.Find("MainControl").GetComponent<MainControl>();
        player=GameObject.Find("Player").transform;
    }

    private void startCooldown() {
        cdLeft=cd;
        isCooling=true;
        cdIcon.SetActive(true);
    }
    public void resetCooldown() {
        isCooling=false;
        cdIcon.SetActive(false);
    }

    private bool teacherTurnedAround() {
        return control.teacherTurnedAround();
    }
    private void act(string clip="Player_Writing") {
        if(clip!=this.clip) {
            pAnim.act(clip, -xOffset, -yOffset);
            audioSource.Stop();
        }
        else {
            pAnim.act(clip, xOffset, yOffset);
            audioSource.PlayOneShot(audioClip);
        }
    }

    public bool check() {
        if(isCooling) {
            cdLeft-=Time.deltaTime;
        }
        if(Input.GetKey(actionKey) && !isCooling) {
            if(control.playerActing!=0 && control.playerActing!=(int)actionKey) {
                return false;
            }
            finish=false;
            if(!acting) {
                act(clip);
                control.playerActing=(int)actionKey;
            }
            acting=true;
            heldTime+=Time.deltaTime;
            if(control.teacherTurnedAround()) {
                Debug.Log("Get caught: "+actionKey);
                control.getCaught();
                audioSource.Stop();
                startCooldown();
                heldTime=0;
                control.playerActing=0;
                finish=true;
            }
            else if(heldTime>=holdFor) {
                act();
                Debug.Log("Action finished: "+actionKey);
                control.level.marks+=marks;
                Debug.Log("Marks: "+control.level.marks);
                startCooldown();
                heldTime=0;
                control.playerActing=0;
                finish=true;
            }
        }
        else {
            acting=false;
            heldTime=0;
            if(!finish) {
                act();
                Debug.Log("Action unfinshed: "+actionKey);
                startCooldown();
                control.playerActing=0;
                finish=true;
            }
        }
        if(isCooling) {
            if(cdLeft<=0) {
                Debug.Log("Cooled down: "+actionKey);
                isCooling=false;
                cdIcon.SetActive(false);
            }
        }
        return acting;
    }
}
