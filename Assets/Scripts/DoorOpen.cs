using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public Vector3 offset;
    public Vector3 initialPos;
    public float moveSpeed = 4f;
    
    private AudioSource _audioSource;
    private MeshRenderer _meshRenderer;
    private readonly Dictionary<Transform, int> _overlapCounts = new Dictionary<Transform, int>();
    private bool _isOpen;
    
    
    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.position;
        _audioSource = GetComponent<AudioSource>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position == initialPos + offset)
        {
            _meshRenderer.enabled = false;
        }
        else
        {
            _meshRenderer.enabled = true;
        }
        Vector3 targetPos = _isOpen ? initialPos + offset : initialPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTarget(other))
        {
            return;
        }

        Transform root = GetTargetRoot(other);
        if (_overlapCounts.ContainsKey(root))
        {
            _overlapCounts[root]++;
        }
        else
        {
            _overlapCounts[root] = 1;
        }

        SetDoorState(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsValidTarget(other))
        {
            return;
        }

        SetDoorState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidTarget(other))
        {
            return;
        }

        Transform root = GetTargetRoot(other);
        if (_overlapCounts.ContainsKey(root))
        {
            _overlapCounts[root]--;
            if (_overlapCounts[root] <= 0)
            {
                _overlapCounts.Remove(root);
            }
        }

        SetDoorState(_overlapCounts.Count > 0);
    }

    private bool IsValidTarget(Collider other)
    {
        return other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Drone");
    }

    private Transform GetTargetRoot(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            return other.attachedRigidbody.transform;
        }

        return other.transform.root;
    }

    private void SetDoorState(bool open)
    {
        if (_isOpen == open)
        {
            return;
        }

        _isOpen = open;

        if (_audioSource != null)
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
            _audioSource.Play();
        }
    }
}
