using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Ocean;
public struct Wave {
    public Vector2 direction;
    public Vector2 origin;
    public float frequency;
    public float amplitude;
    public float phase;
    public float steepness;
    public WaveType waveType;
    

    public Wave(float wavelength, float amplitude, float speed, float direction, float steepness, WaveType waveType, Vector2 origin, WaveFunction waveFunction) {
        this.frequency = 2.0f / wavelength;
        this.amplitude = amplitude;
        this.phase = speed * 2.0f / wavelength;

        if (waveFunction == WaveFunction.Gerstner)
            this.steepness = (steepness - 1) / this.frequency * this.amplitude * 4.0f;
        else
            this.steepness = steepness;

        this.waveType = waveType;
        this.origin = origin;

        this.direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * direction), Mathf.Sin(Mathf.Deg2Rad * direction));
        this.direction.Normalize();
    }

    public Vector2 GetDirection(Vector3 v) {
        Vector2 d = this.direction;

        if (waveType == WaveType.Circular) {
            Vector2 p = new Vector2(v.x, v.z);

            Vector2 heading = p - this.origin;
            d = p - this.origin;
            d.Normalize();
        }

        return d;
    }

    public float GetWaveCoord(Vector3 v, Vector2 d) {
        if (waveType == WaveType.Circular) {
            Vector2 p = new Vector2(v.x, v.z);
            Vector2 heading = p - this.origin;

            return heading.magnitude;
        }

        return v.x * d.x + v.z * d.y;
    }

    public float GetTime() {
        return waveType == WaveType.Circular ? -Time.time * this.phase : Time.time * this.phase;
    }

    public float Sine(Vector3 v) {
        Vector2 d = GetDirection(v);
        float xz = GetWaveCoord(v, d);

        return Mathf.Sin(this.frequency * xz + GetTime()) * this.amplitude;
    }

    public Vector3 SineNormal(Vector3 v) {
        Vector2 d = GetDirection(v);
        float xz = GetWaveCoord(v, d);

        float dx = this.frequency * this.amplitude * d.x * Mathf.Cos(xz * this.frequency + GetTime());
        float dy = this.frequency * this.amplitude * d.y * Mathf.Cos(xz * this.frequency + GetTime());

        return new Vector3(dx, dy, 0.0f);
    }

    public float SteepSine(Vector3 v) {
        Vector2 d = GetDirection(v);
        float xz = GetWaveCoord(v, d);

        return 2 * this.amplitude * Mathf.Pow((Mathf.Sin(xz * this.frequency + GetTime()) + 1) / 2.0f, this.steepness);
    }

    public Vector3 SteepSineNormal(Vector3 v) {
        Vector2 d = GetDirection(v);
        float xz = GetWaveCoord(v, d);

        float h = Mathf.Pow((Mathf.Sin(xz * this.frequency + GetTime()) + 1) / 2.0f, this.steepness - 1);
        float dx = this.steepness * d.x * this.frequency * this.amplitude * h * Mathf.Cos(xz * this.frequency + GetTime());
        float dy = this.steepness * d.y * this.frequency * this.amplitude * h * Mathf.Cos(xz * this.frequency + GetTime());

        return new Vector3(dx, dy, 0.0f);
    }

    public Vector3 Gerstner(Vector3 v) {
        Vector2 d = GetDirection(v);
        float xz = GetWaveCoord(v, d);

        Vector3 g = new Vector3(0.0f, 0.0f, 0.0f);
        g.x = this.steepness * this.amplitude * d.x * Mathf.Cos(this.frequency * xz + GetTime());
        g.z = this.steepness * this.amplitude * d.y * Mathf.Cos(this.frequency * xz + GetTime());
        g.y = this.amplitude * Mathf.Sin(this.frequency * xz + GetTime());

        return g;
    }

    public Vector3 GerstnerNormal(Vector3 v) {
        Vector2 d = GetDirection(v);
        float xz = GetWaveCoord(v, d);

        Vector3 n = new Vector3(0.0f, 0.0f, 0.0f);

        float wa = this.frequency * this.amplitude;
        float s = Mathf.Sin(this.frequency * xz + GetTime());
        float c = Mathf.Cos(this.frequency * xz + GetTime());

        n.x = d.x * wa * c;
        n.z = d.y * wa * c;
        n.y = this.steepness * wa * s;

        return n;
    }
}