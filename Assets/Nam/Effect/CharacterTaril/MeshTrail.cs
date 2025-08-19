using UnityEngine;
using System.Collections;

namespace MySampleEx
{
    public class MeshTrail : MonoBehaviour
    {
        public float activeTime = 2f;               //이펙트 지속 시간
        public float meshRefreshRate = 0.1f;        //잔상 생성 간격
        public float meshDestroyDelay = 3f;         //잔상 킬 딜레이 시간

        public Material trailMat;                   //잔상 효과 메터리얼
        public string shaderVarRef;
        public float shaderVarRate = 0.1f;
        public float shaderRefreshRate = 0.05f;

        private bool isTrailActive;
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        private Transform positionToSpawn;

        private void Start()
        {
            positionToSpawn = transform;
        }

        private void Update()
        {
            if(Input.GetMouseButtonDown(1))
            {
                ActivateTrail();
            }
        }

        public void ActivateTrail()
        {
            StartCoroutine(ActivateTrail2(activeTime));
        }

        IEnumerator ActivateTrail2(float timeActive)
        {
            while(timeActive > 0)
            {
                timeActive -= meshRefreshRate;

                if(skinnedMeshRenderers == null )
                {
                    skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                }

                for( int i = 0; i < skinnedMeshRenderers.Length; i++ )
                {
                    GameObject gObj = new GameObject();
                    gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                    MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                    MeshFilter mf = gObj.AddComponent<MeshFilter>();

                    Mesh mesh = new Mesh();
                    skinnedMeshRenderers[i].BakeMesh(mesh);

                    mf.mesh = mesh;
                    mr.material = trailMat;

                    StartCoroutine(AnimateMaterialFloat(mr.material, 0f, shaderVarRate, shaderRefreshRate));

                    Destroy(gObj, meshDestroyDelay);
                }

                yield return new WaitForSeconds(meshRefreshRate);
            }
        }

        IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
        {
            float valueToAnimate = mat.GetFloat(shaderVarRef);

            while(valueToAnimate > goal)
            {
                valueToAnimate -= rate;
                mat.SetFloat(shaderVarRef, valueToAnimate);
                yield return new WaitForSeconds(refreshRate);
            }
        }
    }
}