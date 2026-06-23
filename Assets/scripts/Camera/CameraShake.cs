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
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        originalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (shakeDuration > 0)
        {
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
            transform.localPosition = originalPos + new Vector3(shakeOffset.x, shakeOffset.y, 0f);    
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }
}