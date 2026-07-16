using UnityEngine;

namespace AstroScope
{
	/// <summary>
	/// Keeps the attached transform facing the main camera so world-space labels stay readable.
	/// </summary>
	public class Billboard : MonoBehaviour
	{
		private Camera _targetCamera;

		private void LateUpdate()
		{
			if (_targetCamera == null)
			{
				_targetCamera = Camera.main;
				if (_targetCamera == null)
				{
					return;
				}
			}

			transform.rotation = Quaternion.LookRotation(transform.position - _targetCamera.transform.position);
		}
	}
}
