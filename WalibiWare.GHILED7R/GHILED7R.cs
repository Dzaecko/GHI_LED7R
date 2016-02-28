using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI.Xaml;

namespace WalibiWare.GHILED7R
{
    public class GHILED7R
    {
        public enum Direction
        {
            LEFT, 
            RIGHT
        }

        public enum Speed
        {
            SLOW,
            NORMAL,
            FAST
        }

        public enum LED
        {
            LED_1 = 0x01,
            LED_2 = 0x02,
            LED_3 = 0x04,
            LED_4 = 0x08,
            LED_5 = 0x10,
            LED_6 = 0x20,
            LED_RED = 0x40
        }

        private const string I2C_NAME = "I2C1"; //known name for RPI2
        private const byte EXPANDER_ADDRESS = 0x20; // 16-bit expander address           
        private byte BUS_A = 0x12; //using byte for BUS A on MCP23017 
        private bool shouldRun;
        private I2cDevice i2cMCP23017; //I2C Device for the MCP23017
        Task rotateTask;  

        public GHILED7R()
        {
           InitI2C();
          
        }

        private async void InitI2C()
        {
            var i2cSettings = new I2cConnectionSettings(EXPANDER_ADDRESS);
            string deviceSelector = I2cDevice.GetDeviceSelector(I2C_NAME);
            var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
            i2cMCP23017 = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);

            i2cMCP23017.Write(new byte[] {0x00, 0x00}); // set all pins from bus a to output 
            
            TurnOn(LED.LED_1);
            TurnOff();                                    
        }

        public void TurnOn(LED led)
        {
            shouldRun = false;
            i2cMCP23017.Write(new byte[] { BUS_A, (byte) led });
        }

        public void TurnOff()
        {
            i2cMCP23017.Write(new byte[] { BUS_A, 0x00 });
        }

        public void Rotate(Direction direction, Speed speed)
        {
            shouldRun = true;
            int slowTimer; // set delay
            int position; // set position for led
            switch (speed)
            {
                case (Speed) 0:
                    slowTimer = 800; // set delay to 800 ms
                    break;
                case (Speed)1:
                    slowTimer = 400; // set delay to 400 ms
                    break;
                case (Speed) 2:
                    slowTimer = 100; // set delay to 100
                    break;
                default:
                    slowTimer = 400; // default is 400 ms
                    break;
            }

            position = direction == 0 ? 32 : 1; // check direction for start led
                       
            while (shouldRun)
            {
                i2cMCP23017.Write(new byte[] { BUS_A, (byte)position });
                Task.Delay(slowTimer).Wait();

                if (direction == 0)
                {
                    position = position == 1 ? 32 : position >> 1;
                }
                else
                {
                    position = position == 32 ? 1 : position << 1;
                }
              

            }

        }
    }
}
