using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
[RequireComponent(typeof(Light))]
public class MuzzleFlash : MonoBehaviour {
    private VisualEffect _vfx;
    private Light _light;
    private float _lightDefaultIntensity = 0f;
    private float _lightCurrentIntensity = 0f;
    private bool _isLightInControl = false;

    private void Awake() {
        _vfx = GetComponent<VisualEffect>();
        _light = GetComponent<Light>();
        _lightDefaultIntensity = _light.intensity;
        _light.intensity = 0f;
    }

    private IEnumerator ControlLight() {
        _lightCurrentIntensity = _lightDefaultIntensity;
        while (_lightCurrentIntensity > 0f) {
            _lightCurrentIntensity = Mathf.Max(_lightCurrentIntensity - _lightDefaultIntensity * Time.deltaTime * 10f, 0f);
            _light.intensity = _lightCurrentIntensity;
            yield return new WaitForEndOfFrame();
        }
        _light.enabled = false;
        _isLightInControl = false;
    }

    public void Play() {
        _vfx.Play();
        if (!_isLightInControl) {
            _isLightInControl = true;
            _light.enabled = true;
            StartCoroutine(ControlLight());
        }
        else {
            _lightCurrentIntensity = _lightDefaultIntensity;
        }
    }
}
