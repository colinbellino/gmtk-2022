using UnityEngine;

public class Bag : MonoBehaviour
{
	[SerializeField] private SpriteRenderer _spriteRenderer;

	private Color _defaultColor;

	private void Awake()
	{
		_defaultColor = _spriteRenderer.color;
	}

	public void Dehighlight()
	{
		_spriteRenderer.color = _defaultColor;
	}

	public void Highlight()
	{
		var color = _defaultColor;
		color.r = 1;
		_spriteRenderer.color = color;
	}
}
