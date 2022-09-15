using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    //Flight Variables
    public DroneController _DroneController;

    public Button _FlyButton;
    public Button _LandButton;
    public Button _ChirpButton;
    public Button _PiercerButton;
    public Button _SirenOn;
    public Button _SirenOff;

    public GameObject _Controls;

    //Sound Var
    private int currentSound = 0;

    //AR
    public ARRaycastManager _RaycastManager;
    public ARPlaneManager _PlaneManager;
    List<ARRaycastHit> _HitResult = new List<ARRaycastHit>();

    public GameObject _Drone;

    struct DroneAnimationControls
    {
        public bool _moving;
        public bool _interpolatingAsc;
        public bool _interpolatingDes;
        public float _axis;
        public float _direction;
    }

    DroneAnimationControls _MovingLeft;
    DroneAnimationControls _MovingBack;

    // Start is called before the first frame update
    void Start()
    {
        _LandButton.gameObject.SetActive(false);
        _SirenOff.gameObject.SetActive(false);
        _FlyButton.onClick.AddListener(EventOnClickFLyButton);
        _LandButton.onClick.AddListener(EventOnClickLandButton);
        _ChirpButton.onClick.AddListener(OnClickChirpButton);
        _SirenOn.onClick.AddListener(OnClickSirenOnButton);
    }

    // Update is called once per frame
    void Update()
    {
        //update sound for flying
        if (_DroneController.IsIdle())
        {
            //currentSound = 0 --> do nothing.
        }
        else if (_DroneController.IsTakingOff())
        {
            if (currentSound == 0)
            {
                currentSound = 1;

                _DroneController.PlayTakeOffSFX();
                //FindObjectOfType<AudioManager>().Play("DroneStartSound");
            }
        }
        else if (_DroneController.IsFlying())
        {
            if (currentSound == 1)
            {
                currentSound = 2;

                _DroneController.StoptakeOffSFX();
                _DroneController.PlayFlyingSFX();
                //FindObjectOfType<AudioManager>().StopPlay("DroneStartSound");
                //FindObjectOfType<AudioManager>().Play("Flying");
            }
        }
        else if (_DroneController.IsLanding())
        {
            if (currentSound == 2)
            {
                currentSound = 0;

                _DroneController.StopFlyingSFX();
                _DroneController.PlayLandingSFX();
                //FindObjectOfType<AudioManager>().StopPlay("Flying");
                //FindObjectOfType<AudioManager>().Play("Landing");
            }
        }

        UpdateControls(ref _MovingLeft);
        UpdateControls(ref _MovingBack);

        _DroneController.Move(_MovingLeft._axis * _MovingLeft._direction, _MovingBack._axis * _MovingBack._direction);

        if (_DroneController.IsIdle())
        {
            UpdateAR();
        }
    }

    void UpdateAR()
    {
        Vector2 positionScreenSpace = Camera.current.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));

        _RaycastManager.Raycast(positionScreenSpace, _HitResult, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinBounds);

        if (_HitResult.Count > 0)
        {
            if (_PlaneManager.GetPlane(_HitResult[0].trackableId).alignment == UnityEngine.XR.ARSubsystems.PlaneAlignment.HorizontalUp)
            {
                Pose pose = _HitResult[0].pose;
                _Drone.transform.position = pose.position;
                _Drone.SetActive(true);
            }
        }
    }

    void UpdateControls(ref DroneAnimationControls _controls)
    {
        if (_controls._moving || _controls._interpolatingAsc || _controls._interpolatingDes)
        {
            if (_controls._interpolatingAsc)
            {
                _controls._axis += 0.05f;

                if (_controls._axis >= 1.0f)
                {
                    _controls._axis = 1.0f;
                    _controls._interpolatingAsc = false;
                    _controls._interpolatingDes = true;
                }
            }
            else if (!_controls._moving)
            {
                _controls._axis -= 0.05f;

                if (_controls._axis <= 0.0f)
                {
                    _controls._axis = 0.0f;
                    _controls._interpolatingDes = false;
                }
            }
        }
    }

    //siren sfx

    void OnClickChirpButton()
    {
        if (_DroneController.IsChirping())
        {
            _DroneController.StopChirpSFX();
        }
        if (_DroneController.SirenOn())
        {
            _DroneController.StopSiren();
            _SirenOff.gameObject.SetActive(false);
            _SirenOn.gameObject.SetActive(true);
        }
        _DroneController.PlayChirpSFX();
    }

    public void PiercerButtonPressed()
    {
        if (_DroneController.SirenOn())
        {
            _DroneController.PauseSiren();
        }
        _DroneController.StopChirpSFX();
        _DroneController.PlayPiercerSFX();
    }

    public void PiercerButtonReleased()
    {
        _DroneController.StopPiercerSFX();
        if (_DroneController.SirenPaused())
        {
            _DroneController.PlaySiren();
        }
    }

    void OnClickSirenOnButton()
    {
        _DroneController.StopChirpSFX();
        _DroneController.PlaySiren();
        _SirenOff.gameObject.SetActive(true);
    }

    public void OnClickSirenOffButton()
    {
        _DroneController.StopSiren();
        _SirenOff.gameObject.SetActive(false);
        _SirenOn.gameObject.SetActive(true);
    }

    

    //takeoff and landing

    void EventOnClickFLyButton()
    {
        if (_DroneController.IsIdle())
        {
            _DroneController.TakeOff();
            _FlyButton.gameObject.SetActive(false);
            _LandButton.gameObject.SetActive(true);
            _Controls.SetActive(true);
        }
    }

    void EventOnClickLandButton()
    {
        if (_DroneController.IsFlying())
        {
            _DroneController.Land();
            _FlyButton.gameObject.SetActive(true);
            _LandButton.gameObject.SetActive(false);
            _Controls.SetActive(false);
        }
    }

    //horizontal movement
    public void EventOnLeftButtonPressed()
    {
        _MovingLeft._moving = true;
        _MovingLeft._interpolatingAsc = true;
        _MovingLeft._direction = -1.0f;
    }

    public void EventOnLeftButtonReleased()
    {
        _MovingLeft._moving = false;    
    }

    public void EventOnRightButtonPressed()
    {
        _MovingLeft._moving = true;
        _MovingLeft._interpolatingAsc = true;
        _MovingLeft._direction = 1.0f;
    }

    public void EventOnRightButtonReleased()
    {
        _MovingLeft._moving = false;
    }

    //vertical movement
    public void EventOnForwardButtonPressed()
    {
        _MovingBack._moving = true;
        _MovingBack._interpolatingAsc = true;
        _MovingBack._direction = 1.0f;
    }

    public void EventOnForwardButtonReleased()
    {
        _MovingBack._moving = false;
    }

    public void EventOnBackwardButtonPressed()
    {
        _MovingBack._moving = true;
        _MovingBack._interpolatingAsc = true;
        _MovingBack._direction = -1.0f;
    }

    public void EventOnBackwardButtonReleased()
    {
        _MovingBack._moving = false;
    }
}
