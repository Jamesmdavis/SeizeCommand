using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SeizeCommand.References;

namespace SeizeCommand.Utility
{
    //This script Mirrors the Dynamic Players Movement and Rotation
    [RequireComponent(typeof(GameObjectReference))]
    public class MirrorTransform : MonoBehaviour
    {
        [SerializeField] private bool mirrorPosition;
        [SerializeField] private bool mirrorRotation;

        private Transform mirrorTarget;
        private Coroutine coMirrorTransform;
        private bool isMirroring;

        private IEnumerator CoMirrorTransform()
        {
            while(true)
            {
                isMirroring = true;

                if(!mirrorTarget)
                {
                    isMirroring = false;
                    StopCoroutine(coMirrorTransform);
                    Destroy(gameObject);
                }
                else
                {  
                    transform.localPosition = mirrorPosition ? mirrorTarget.localPosition
                        : transform.localPosition;

                    transform.localRotation = mirrorRotation ? mirrorTarget.localRotation
                        : transform.localRotation;
                }
                
                yield return null;
            }
        }

        public void StartMirroring()
        {
            mirrorTarget = GetComponent<GameObjectReference>().Reference.transform;
            coMirrorTransform = StartCoroutine(CoMirrorTransform());
        }

        private void OnDisable()
        {
            StopCoroutine(coMirrorTransform);
        }
    }
}