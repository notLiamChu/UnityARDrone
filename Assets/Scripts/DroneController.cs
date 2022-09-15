using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Audio;

public class DroneController : MonoBehaviour
{
    //Sound var
    AudioSource flyingSFX;
    AudioSource takeOffSFX;
    AudioSource landingSFX;
    AudioSource chirpSFX;
    AudioSource piercerSFX;
    AudioSource sirenSFX;

    private bool chirping = false;
    private bool piercing = false;
    private bool sirenOn = false;
    private bool sirenPaused = false;

    //States
    enum DroneState
    {
        DRONE_STATE_IDLE,
        DRONE_STATE_START_TAKEINGOFF,
        DRONE_STATE_TAKINGOFF,
        DRONE_STATE_MOVING_UP,
        DRONE_STATE_FLYING,
        DRONE_STATE_START_LANDING,
        DRONE_STATE_LANDING,
        DRONE_STATE_LANDED,
        DRONE_STATE_WAIT_ENGINE_STOP
    }

    DroneState _State;

    Animator _Anim;

    UnityEngine.Vector3 _Speed = new UnityEngine.Vector3(0.0f, 0.0f, 0.0f);

    public float _SpeedMultiplier = 1.0f;

    public bool IsIdle()
    {
        return (_State == DroneState.DRONE_STATE_IDLE);
    }

    public void TakeOff()
    {
        _State = DroneState.DRONE_STATE_START_TAKEINGOFF;
    }

    public bool IsFlying()
    {
        return (_State == DroneState.DRONE_STATE_FLYING);
    }

    public bool IsTakingOff()
    {
        return (_State == DroneState.DRONE_STATE_START_TAKEINGOFF);
    }

    public bool IsLanding()
    {
        return (_State == DroneState.DRONE_STATE_START_LANDING);
    }

    public void Land()
    {
        _State = DroneState.DRONE_STATE_START_LANDING;
    }

    void Start()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        flyingSFX = audioSources[0];
        sirenSFX = audioSources[1];
        landingSFX = audioSources[2];
        takeOffSFX = audioSources[3];
        chirpSFX = audioSources[4];
        piercerSFX = audioSources[5];


        _Anim = GetComponent<Animator>();

        _State = DroneState.DRONE_STATE_IDLE;
    }

    //Sound methods
    public void PlayFlyingSFX()
    {
        flyingSFX.Play();
    }

    public void StopFlyingSFX()
    {
        flyingSFX.Stop();
    }

    public void PlayLandingSFX()
    {
        landingSFX.Play();
    }

    public void StopLandingSFX()
    {
        landingSFX.Stop();
    }

    public void PlayTakeOffSFX()
    {
        takeOffSFX.Play();
    }

    public void StoptakeOffSFX()
    {
        takeOffSFX.Stop();
    }

    //siren sfx

    public bool IsChirping()
    {
        return chirping;
    }

    public void PlayChirpSFX()
    {
        chirping = true;
        chirpSFX.Play();
    }

    public void StopChirpSFX()
    {
        chirping = false;
        chirpSFX.Stop();
    }

    public bool IsPiercing()
    {
        return piercing;
    }

    public void PlayPiercerSFX()
    {
        piercing = true;
        piercerSFX.Play();
    }

    public void StopPiercerSFX()
    {
        piercing = false;
        piercerSFX.Stop();
    }

    public bool SirenOn()
    {
        return sirenOn;
    }

    public bool SirenPaused()
    {
        return sirenPaused;
    }

    public void PlaySiren()
    {
        sirenOn = true;
        sirenPaused = false;
        sirenSFX.Play();
    }

    public void StopSiren()
    {
        sirenOn = false;
        sirenSFX.Stop();
    }

    public void PauseSiren()
    {
        sirenSFX.Pause();
        sirenPaused = true;
    }

    //Move

    public void Move(float _speedX, float _speedZ)
    {   
        _Speed.x = _speedX;
        _Speed.z = _speedZ;

        UpdateDrone();
    }

    // Update is called once per frame
    void UpdateDrone()
    {
        switch (_State)
        {
            case DroneState.DRONE_STATE_IDLE:
                break;
            case DroneState.DRONE_STATE_START_TAKEINGOFF:
                _Anim.SetBool("TakeOff", true);
                _State = DroneState.DRONE_STATE_TAKINGOFF;
                break;

            case DroneState.DRONE_STATE_TAKINGOFF:
                if (_Anim.GetBool("TakeOff") == false)
                {
                    _State = DroneState.DRONE_STATE_MOVING_UP;
                }
                break;

            case DroneState.DRONE_STATE_MOVING_UP:
                if (_Anim.GetBool("MoveUp") == false)
                {
                    _State = DroneState.DRONE_STATE_FLYING; 
                }
                break;

            case DroneState.DRONE_STATE_FLYING:
                float angleZ = -30.0f * _Speed.x * 60.0f * Time.deltaTime;
                float angleX = 30.0f * _Speed.z * 60.0f * Time.deltaTime;

                UnityEngine.Vector3 rotation = transform.localRotation.eulerAngles;

                transform.localPosition += _Speed * _SpeedMultiplier * Time.deltaTime;
                transform.localRotation = UnityEngine.Quaternion.Euler(angleX, rotation.y, angleZ);
                break;

            case DroneState.DRONE_STATE_START_LANDING:
                _Anim.SetBool("MoveDown", true);
                _State = DroneState.DRONE_STATE_LANDING;
                break;

            case DroneState.DRONE_STATE_LANDING:
                if (_Anim.GetBool("MoveDown") == false)
                {
                    _State = DroneState.DRONE_STATE_LANDED;
                }
                break;

            case DroneState.DRONE_STATE_LANDED:
                _Anim.SetBool("Land", true);
                _State = DroneState.DRONE_STATE_WAIT_ENGINE_STOP;
                break;

            case DroneState.DRONE_STATE_WAIT_ENGINE_STOP:
                if (_Anim.GetBool("Land") == false)
                {
                    _State = DroneState.DRONE_STATE_IDLE;
                }
                break;
        }
    }
}
