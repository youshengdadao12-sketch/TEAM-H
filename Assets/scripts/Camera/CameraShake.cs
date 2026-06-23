//被弾演出
using UnityEngine;

public class PlayerShake : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.5f;//揺れの時間
    [SerializeField] private float shakeMagnitude =1;//揺れの大きさ
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
        TriggerShake(shakeDuration, shakeMagnitude);
        originalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (shakeDuration > 0)
        {
            transform.LocalPosition = originalPos + random.insideUnitCricle*shakeMagnitude;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = originalPos;
        }
    }
}