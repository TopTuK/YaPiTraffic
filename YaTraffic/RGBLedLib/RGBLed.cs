using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RGBLedLib
{
    public enum RGBLedError
    {
        E_UNKNOWN = 0,
        E_GPIO_NOT_FOUND,
        E_OPEN_PIN_ERROR
    }

    public class RGBLedException : Exception
    {
        public readonly RGBLedError ErrorType = RGBLedError.E_UNKNOWN;

        public RGBLedException(RGBLedError errorType) :
            base()
        {
            ErrorType = errorType;
        }

        public RGBLedException(RGBLedError errorType, Exception innerException) :
            base("", innerException)
        {
            ErrorType = errorType;
        }
    }

    public enum RGBLedColor
    {
        UNDEFINED,
        RED, GREEN, BLUE
    }

    public class RGBLed : IDisposable
    {
        private GpioPin m_redPin = null;
        private GpioPin m_greenPin = null;
        private GpioPin m_bluePin = null;

        private RGBLedColor m_ledColor = RGBLedColor.UNDEFINED;

        public RGBLed(int redPin, int greenPin, int bluePin)
        {
            var gpio = GpioController.GetDefault();
            if (gpio == null) throw new RGBLedException(RGBLedError.E_GPIO_NOT_FOUND);

            try
            {
                m_redPin = gpio.OpenPin(redPin);
                m_greenPin = gpio.OpenPin(greenPin);
                m_bluePin = gpio.OpenPin(bluePin);

                m_redPin.Write(GpioPinValue.Low);
                m_redPin.SetDriveMode(GpioPinDriveMode.Output);

                m_greenPin.Write(GpioPinValue.Low);
                m_greenPin.SetDriveMode(GpioPinDriveMode.Output);

                m_bluePin.Write(GpioPinValue.Low);
                m_bluePin.SetDriveMode(GpioPinDriveMode.Output);

            }
            catch(Exception ex)
            {
                throw new RGBLedException(RGBLedError.E_OPEN_PIN_ERROR, ex);
            }
        }

        private void SwitchColor(RGBLedColor ledColor)
        {
            if (m_ledColor == ledColor) return;

            switch(ledColor)
            {
                case RGBLedColor.RED:
                    m_redPin.Write(GpioPinValue.High);
                    m_greenPin.Write(GpioPinValue.Low);
                    m_bluePin.Write(GpioPinValue.Low);
                    break;
                case RGBLedColor.GREEN:
                    m_redPin.Write(GpioPinValue.Low);
                    m_greenPin.Write(GpioPinValue.High);
                    m_bluePin.Write(GpioPinValue.Low);
                    break;
                case RGBLedColor.BLUE:
                    m_redPin.Write(GpioPinValue.Low);
                    m_greenPin.Write(GpioPinValue.Low);
                    m_bluePin.Write(GpioPinValue.High);
                    break;
                default:
                    SwitchOff();
                    break;
            }

            m_ledColor = ledColor;
        }

        public void SwitchOff()
        {
            m_redPin.Write(GpioPinValue.High);
            m_greenPin.Write(GpioPinValue.High);
            m_bluePin.Write(GpioPinValue.High);
        }

        public void Dispose()
        {
            m_redPin.Dispose();
            m_greenPin.Dispose();
            m_bluePin.Dispose();
        }

        public RGBLedColor LedColor
        {
            get { return m_ledColor; }
            set
            {
                SwitchColor(value);
            }
        }
    }
}
