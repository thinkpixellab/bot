using System;
using System.Windows.Controls;

namespace PixelLab.SL
{
    public class DiscreteSlider : Slider
    {
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            if (!m_busy)
            {
                m_busy = true;

                if (SmallChange != 0)
                {
                    double newDiscreteValue = (int)(Math.Round(newValue / SmallChange)) * SmallChange;

                    if (newDiscreteValue != m_discreteValue)
                    {
                        Value = newDiscreteValue;
                        base.OnValueChanged(m_discreteValue, newDiscreteValue);
                        m_discreteValue = newDiscreteValue;
                    }
                }
                else
                {
                    base.OnValueChanged(oldValue, newValue);
                }

                m_busy = false;
            }
        }

        bool m_busy;
        double m_discreteValue;
    }
}