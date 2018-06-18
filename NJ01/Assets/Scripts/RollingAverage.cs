
public class RollingAverage
{
    public float CurrentAverage = 0;
    public float[] PrevValues;

    public int CurrentIndex = 0;

    public void Create(int size)
    {
        PrevValues = new float[size];
    }

    public void AddValue(float value)
    {
        PrevValues[CurrentIndex++] = value;
        CurrentIndex %= PrevValues.Length;

        CurrentAverage = 0.0f;
        for (int i = 0; i < PrevValues.Length; ++i)
        {
            CurrentAverage += PrevValues[i];
        }

        CurrentAverage /= PrevValues.Length;
    }

    public void Clear()
    {
        for (int i = 0; i < PrevValues.Length; ++i)
        {
            PrevValues[i] = 0.0f;
        }

        CurrentIndex = 0;
        CurrentAverage = 0.0f;
    }
}
