﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Clip : MonoBehaviour, IDragOwner, IBeginDragHandler, IEndDragHandler, IDragHandler, IClipHandler {

	[System.Serializable]
	public class DragEvent : UnityEvent<float> { }

	[SerializeField]
	private DragEvent _onDragLeft;
	[SerializeField]
	private DragEvent _onDragRight;
	[SerializeField]
	private DragEvent _onDragTop;
	[SerializeField]
	private DragEvent _onDragBottom;

	private RectTransform _rectTransform;

	private ClipData _clipData;

	private int _startFrame;
	private int _endFrame;
	public Canvas canvas;

	private Timebar _owner;

	public Timebar Owner {
		get {
			return _owner;
		}
		set {
			_owner = value;
		}
	}

	private void Awake() {
		_rectTransform = this.GetComponent<RectTransform>();
		DragHandler[] dragHandlers = this.GetComponentsInChildren<DragHandler>();
		for(int i = 0; i < dragHandlers.Length; i++) {
			dragHandlers[i].SetOwner(this);
		}
	}

	private void OnDestroy() {
		_owner.RemoveClip(this);
	}

	public void SetClip(ClipData clipData) {
		this._clipData = clipData;
		SetWidth(clipData.Duration);
	}

	public void DragLeft(PointerEventData eventData) {
		float amount = eventData.delta.x;
		if(_onDragLeft != null) {
			_onDragLeft.Invoke(amount);
		}
		ResizeClip(DragHandler.Direction.Left, eventData);
	}

	public void DragRight(PointerEventData eventData) {
		float amount = eventData.delta.x;
		if(_onDragRight != null) {
			_onDragRight.Invoke(amount);
		}
		ResizeClip(DragHandler.Direction.Right, eventData);
	}

	public void DragTop(PointerEventData eventData) {
		float amount = eventData.delta.y;
		if(_onDragTop != null) {
			_onDragTop.Invoke(amount);
		}
		ResizeClip(DragHandler.Direction.Top, eventData);
	}

	public void DragBottom(PointerEventData eventData) {
		float amount = eventData.delta.y;
		if(_onDragBottom != null) {
			_onDragBottom.Invoke(amount);
		}
		ResizeClip(DragHandler.Direction.Bottom, eventData);
	}

	private void ResizeClip(DragHandler.Direction direction, PointerEventData eventData) {
		Vector2 delta = eventData.delta;
		Vector2 difference = Vector2.zero;

		int multiplier = 1;

		switch (direction)
		{
			case DragHandler.Direction.Bottom: {
				difference =  new Vector2 (0, -delta.y);
				multiplier = -1;
				break;
			}
			case DragHandler.Direction.Right: {
				difference =  new Vector2 (delta.x, 0);
				break;
			}
			case DragHandler.Direction.Left: {
				difference =  new Vector2 (-delta.x, 0);
				multiplier = -1;
				break;
			}
			case DragHandler.Direction.Top: {
				difference =  new Vector2 (0, delta.y);
				break;
			}
		}
		
		_rectTransform.sizeDelta += difference;		
		Vector2 deltaPosition = multiplier * new Vector3(_rectTransform.pivot.x * difference.x, _rectTransform.pivot.y * difference.y, 0);
		_rectTransform.anchoredPosition += deltaPosition;
	}

	public void SetHeight(float height) {
		_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
	}

	public void SetWidth(float width) {
		_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
	}

	public void MovePosition(float deltaX) {
		Debug.Log(deltaX);
		_rectTransform.anchoredPosition += new Vector2(deltaX, 0);
	}

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
		if(PrefabController.ClipDragCurrent) {
			Destroy(PrefabController.ClipDragCurrent.gameObject);
			Destroy(this.gameObject);
		}
    }

    public void OnDrag(PointerEventData eventData)
    {
		eventData.Use();
        
		float deltaY = eventData.position.y - _rectTransform.position.y;
		if(Mathf.Abs(deltaY) > 0.4f * _rectTransform.rect.height && !PrefabController.ClipDragCurrent) {
			// User wants to move clip to different layer!
			PrefabController.ClipDragCurrent = Instantiate(PrefabController.ClipDragPrefab);
			PrefabController.ClipDragCurrent.transform.SetParent(PrefabController.CanvasInstance.transform, false);
			PrefabController.ClipDragCurrent.SetData(_clipData);
		}
		if(PrefabController.ClipDragCurrent) {
			PrefabController.ClipDragCurrent.SetPosition(eventData.position);
			return;
		}
		MovePosition(eventData.delta.x);
    }

    public ClipData GetClip()
    {
        return _clipData;
    }
}
