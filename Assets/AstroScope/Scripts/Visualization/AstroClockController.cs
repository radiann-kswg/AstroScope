using System;
using UnityEngine;

namespace AstroScope
{
	/// <summary>
	/// Drives the simulated observation time shared by the cosmic visualization views.
	/// Advances in real time by default and supports speed multipliers, pausing, and manual jumps.
	/// </summary>
	public class AstroClockController : MonoBehaviour
	{
		[SerializeField]
		private double latitudeDegrees = 35.6895; // Tokyo

		[SerializeField]
		private double longitudeDegrees = 139.6917;

		[SerializeField]
		private string timeZoneId = "Asia/Tokyo";

		[SerializeField]
		private float timeScale = 1f;

		[SerializeField]
		private bool paused;

		private DateTime _simulatedUtc = DateTime.UtcNow;
		private TimeZoneInfo _timeZone;

		/// <summary>Raised when the user jumps or resets the clock so views can recompute immediately.</summary>
		public event Action TimeJumped;

		/// <summary>Observer latitude in degrees (north positive).</summary>
		public double LatitudeDegrees => latitudeDegrees;

		/// <summary>Observer longitude in degrees (east positive).</summary>
		public double LongitudeDegrees => longitudeDegrees;

		/// <summary>Resolved time zone for local time conversion.</summary>
		public TimeZoneInfo TimeZone => _timeZone ??= ResolveTimeZone();

		/// <summary>Current simulated instant in UTC.</summary>
		public DateTime CurrentUtc => _simulatedUtc;

		/// <summary>Current simulated instant converted to the configured local time zone.</summary>
		public DateTime CurrentLocal => TimeZoneInfo.ConvertTime(_simulatedUtc, TimeZone);

		/// <summary>Simulation speed multiplier (1 = real time). Negative values are clamped to zero.</summary>
		public float TimeScale
		{
			get => timeScale;
			set => timeScale = Mathf.Max(0f, value);
		}

		/// <summary>Whether the clock is paused.</summary>
		public bool Paused
		{
			get => paused;
			set => paused = value;
		}

		private void Awake()
		{
			_simulatedUtc = DateTime.UtcNow;
		}

		private void Update()
		{
			if (paused)
			{
				return;
			}

			try
			{
				_simulatedUtc = _simulatedUtc.AddSeconds(Time.deltaTime * timeScale);
			}
			catch (ArgumentOutOfRangeException)
			{
				// The clock reached the DateTime range boundary; stop advancing gracefully.
				paused = true;
			}
		}

		/// <summary>
		/// Offsets the simulated clock by the supplied time span and notifies listeners.
		/// </summary>
		/// <param name="offset">Signed offset to apply.</param>
		public void AddOffset(TimeSpan offset)
		{
			try
			{
				_simulatedUtc = _simulatedUtc.Add(offset);
			}
			catch (ArgumentOutOfRangeException)
			{
				Debug.LogWarning("[AstroScope] Clock offset exceeded the valid DateTime range and was ignored.");
				return;
			}

			TimeJumped?.Invoke();
		}

		/// <summary>
		/// Offsets the simulated clock by whole calendar months and notifies listeners.
		/// </summary>
		/// <param name="months">Signed number of months to add.</param>
		public void AddMonths(int months)
		{
			try
			{
				_simulatedUtc = _simulatedUtc.AddMonths(months);
			}
			catch (ArgumentOutOfRangeException)
			{
				Debug.LogWarning("[AstroScope] Clock month offset exceeded the valid DateTime range and was ignored.");
				return;
			}

			TimeJumped?.Invoke();
		}

		/// <summary>
		/// Resets the simulated clock to the actual current time and notifies listeners.
		/// </summary>
		public void ResetToNow()
		{
			_simulatedUtc = DateTime.UtcNow;
			TimeJumped?.Invoke();
		}

		private TimeZoneInfo ResolveTimeZone()
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			}
			catch (TimeZoneNotFoundException)
			{
				Debug.LogWarning($"[AstroScope] Time zone '{timeZoneId}' not found. Falling back to local system zone.");
				return TimeZoneInfo.Local;
			}
			catch (InvalidTimeZoneException)
			{
				Debug.LogWarning($"[AstroScope] Time zone '{timeZoneId}' invalid. Falling back to local system zone.");
				return TimeZoneInfo.Local;
			}
		}
	}
}
