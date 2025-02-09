﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace TopDownShooter
{
    public class HitPoint : MonoBehaviour
    {
        public float MaxHitPoint = 100;
        public float CurrentHitPoint = 0;

        [Header("PopUpText Settings")] [Tooltip("PopUpText prefab")]
        public GameObject PopUpPrefab;

        [Tooltip("PopUpText Color")] public Color PopUpTextColor = Color.red;
        [Tooltip("PopUpText fade time")] public float FadeTime = 0.5f;

        public Image healthBar;

        public FlashOnDamage FlashOnDamage;

        private float _hitRatio;

        private void Awake()
        {
            CurrentHitPoint = MaxHitPoint;
        }

        public void ApplyDamage(float amount)
        {
            CurrentHitPoint -= amount;
            UpdatePointsBars();

            //if flash component exit start damage flash
            if (FlashOnDamage)
            {
                FlashOnDamage.StartDamageFlash();
            }

            //if pop up component exist instantiate pop up text
            if (PopUpPrefab)
            {
                InstancePopUp(amount.ToString(CultureInfo.InvariantCulture));
            }

            //if current hit point is <= 0 kill the player
            if (CurrentHitPoint <= 0)
            {
                CurrentHitPoint = 0;
                Dead();
            }
        }

        private void UpdatePointsBars()
        {
            _hitRatio = CurrentHitPoint / MaxHitPoint;
            healthBar.fillAmount = _hitRatio;
        }

        //instance the popUp text (demo only)
        //You can change this to text mesh pro or another GUI solutions.
        private void InstancePopUp(string popUpText)
        {
            var poPupText =
                Instantiate(PopUpPrefab, transform.position + Random.insideUnitSphere * 0.4f,
                    transform.rotation);
            Destroy(poPupText, FadeTime);
            poPupText.transform.GetChild(0).GetComponent<TextMesh>().text = popUpText;
            poPupText.transform.GetChild(0).GetComponent<TextMesh>().color = PopUpTextColor;
        }

        private void Dead()
        {
            //you can add dead animation on this place
            Destroy(gameObject, 0);
        }

        //use this for collision bullets
        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.GetComponent<Damage>())
            {
                ApplyDamage(other.transform.GetComponent<Damage>().DamagePower);
            }
        }

        //use this for triggers bullets
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.GetComponent<Damage>())
            {
                ApplyDamage(other.transform.GetComponent<Damage>().DamagePower);
            }
        }
    }
}