using System;
using System.Collections;
using System.Text;


namespace Metec.MVBD
{

    /// <summary>List of virtual keys</summary>
    public class DeviceKeyCollection : IEnumerable
    {

        public static DeviceKeyCollection FromBrailleKeys  (BitString bs)
        {
            DeviceKeyCollection keys = new DeviceKeyCollection();
            keys.SetBrailleKeys( bs.Value );

            return keys;
        }

        public static DeviceKeyCollection FromIAB  (byte value)
        {
            DeviceKeyCollection keys = new DeviceKeyCollection();
            keys.SetIAB( value );

            return keys;
        }

        public static DeviceKeyCollection FromID  (byte id)
        {
            DeviceKeyCollection keys = new DeviceKeyCollection();
            keys[id].IsPressed = true;

            return keys;
        }


        public static DeviceKeyCollection FromID  (byte id1, byte id2)
        {
            DeviceKeyCollection keys = new DeviceKeyCollection();
            keys[id1].IsPressed = true;
            keys[id2].IsPressed = true;

            return keys;
        }


        public static DeviceKeyCollection FromHash (string hash)
        {
            if (hash == null)   return null;

            DeviceKeyCollection keys = new DeviceKeyCollection();

            char[] ca = hash.ToCharArray();

            for(int i = 0; i < ca.Length; i++)
            {
                keys[i].IsPressed = (ca[i] == '1');
            }

            return keys;
        }






        protected DeviceKey[]   _keys;
        protected byte[]        _bit;
        protected long          _value;


        /// <summary>Erstellt die Tasten</summary>
        public DeviceKeyCollection()
        {
            _bit    = new byte[] {1,2,4,8,16,32,64,128};

            // Tasten eintragen
            _keys = new DeviceKey[241];

            for (int i = 0; i < _keys.Length; i++)
            {
                _keys[i] = new DeviceKey ( this, (DeviceKeys)i );
            }


            // Kurztexte setzen:
            for (int i = 0; i < 100; i++)   { _keys[0  +i].ShortText = String.Format("{0:D2}", i+1); }  // IAB
            for (int i = 0; i < 100; i++)   { _keys[100+i].ShortText = String.Format("{0:D2}", i+1); }  // IAB2
            BK1.ShortText = "1";
            BK2.ShortText = "2";
            BK3.ShortText = "3";
            BK4.ShortText = "4";

            BK5.ShortText = "5";
            BK6.ShortText = "6";
            BK7.ShortText = "7";
            BK8.ShortText = "8";

            // Sieht scheiße aus
            //LUp.ShortText     = "↑";
            //LDown.ShortText   = "↓";
            //LLeft.ShortText   = "←";
            //LRight.ShortText  = "→";
            //LSelect.ShortText = "⏎";

            //RUp.ShortText     = "↑";
            //RDown.ShortText   = "↓";
            //RLeft.ShortText   = "←";
            //RRight.ShortText  = "→";
            //RSelect.ShortText = "⏎";

            LUp.ShortText     = "U";
            LDown.ShortText   = "D";
            LLeft.ShortText   = "L";
            LRight.ShortText  = "R";
            LSelect.ShortText = "X";

            RUp.ShortText     = "U";
            RDown.ShortText   = "D";
            RLeft.ShortText   = "L";
            RRight.ShortText  = "R";
            RSelect.ShortText = "X";




            LT.ShortText      = "LT";
            RT.ShortText      = "RT";

            LSpace.ShortText  = "..";
            RSpace.ShortText  = "..";

            LSpecial.ShortText = "<<";
            RSpecial.ShortText = ">>";

            LZoomUp.ShortText    = "+";
            LZoomDown.ShortText  = "-";

            RZoomUp.ShortText    = "+";
            RZoomDown.ShortText  = "-";

            // http://www.alanwood.net/unicode/miscellaneous_technical.html

            MoveUp.ShortText    = "⏶";
            MoveDown.ShortText  = "⏷";
            MoveLeft.ShortText  = "⏴";
            MoveRight.ShortText = "⏵";

            MoveUp2.ShortText    = "⏫";
            MoveDown2.ShortText  = "⏬";
            MoveLeft2.ShortText  = "⏪";
            MoveRight2.ShortText = "⏩";

            Esc.ShortText        = "X";
            Home.ShortText       = "⌂";     // "⌘";
            Back.ShortText       = "←";

            LGesture.ShortText   = "T";     // Touch Geste
            RGesture.ShortText   = "T";     // Touch Geste
        }


        public event DeviceKeyEventHandler  KeyUp;
        public event DeviceKeyEventHandler  KeyDown;


        public virtual void OnKeyUp(DeviceKey key)
        {
            if (KeyUp != null)
            {
                KeyUp(this, new DeviceKeyEventArgs(key));
            }
        }


        public virtual void OnKeyDown(DeviceKey key)
        {
            if (KeyDown != null)
            {
                KeyDown(this, new DeviceKeyEventArgs(key));
            }
        }




        public DeviceKey this[int index] { get { return (DeviceKey)_keys[index]; } }
        public int Count { get { return _keys.Length; } }
        public IEnumerator GetEnumerator() { return _keys.GetEnumerator(); }


        /// <summary>Gibt eine Liste der gedrückten Tasten zurück</summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach(DeviceKey k in _keys)
            {
                if ( k.IsPressed == true )
                { 
                    sb.Append(k.Name);
                    sb.Append("+");
                }
            }


            if (sb.Length != 0) sb.Length -=1;

            return sb.ToString();
        }



        /// <summary>Erstellt einen Hash als String ("1"=Taste gedrückt "0"=Taste losgelassen)</summary>
        public string GetHash()
        {
            char[] ca = new char[ _keys.Length ];

            for(int i = 0; i < _keys.Length; i++)
            {
                if ( _keys[i].IsPressed == true )
                {
                    ca[i] = '1';
                }
                else
                {
                    ca[i] = '0';
                }
            }

            return new string(ca);
        }


        public string AddHash(string hash)
        {
            if (hash == null)   hash = new string('0', _keys.Length);

            char[] ca1 = hash.ToCharArray();
            char[] ca  = new char[_keys.Length];

            for(int i = 0; i < _keys.Length; i++)
            {
                if (ca1[i] == '1')
                {
                    ca[i] = '1';
                }
                else
                { 
                    if ( _keys[i].IsPressed == true )
                    {
                        ca[i] = '1';
                    }
                    else
                    {
                        ca[i] = '0';
                    }
                }
            }

            return new string(ca);
        }





        /// <summary>Sind alle Tasten losgelassen (IsPressed == false)</summary>
        public bool IsZero()
        {
            foreach(DeviceKey k in _keys)
            {
                if (k.IsPressed == true)    return false;
            }

            return true;
        }



        /// <summary>Alle Tasten auf losgelassen setzen. (IsPressed == false)</summary>
        public void Clear()
        {
            foreach(DeviceKey k in _keys)
            {
                k.IsPressed = false;
            }
        }

        /// <summary>Resets all IAB values (0-199)</summary>
        public void ClearIAB() 
        {
            for (int i = 0; i < 200; i++)
            {
                _keys[i].IsPressed = false;
            }
        }



        /// <summary>Returns the value of a IAB. 0-99</summary>
        public bool GetIAB(int index) 
        {
            return _keys[0+index].IsPressed;
        }

        /// <summary>Returns the value the second IAB. 100-199</summary>
        public bool GetIAB2(int index) 
        {
            return _keys[100+index].IsPressed;
        }




        /// <summary>Sets the button on a braille cell. 0=None,1-99,100-199</summary>
        public void   SetIAB  (byte value)
        {
            if      ( value ==    0 )   ClearIAB();             // 0 No Key
            else if ( value <=   99 )   _keys[value-1].IsPressed = true;    // IAB    1-99    --> 0-98
            else if ( value <=  199 )   _keys[value-1].IsPressed = true;    // IAB2   100-199 --> 100-199
            else Debug.Print("Invalid IAB:{0}", value);         // 200... ungültig
        }


        /// <summary>Sets the button on a braille cell with the bits from the byte array</summary>
        public void   SetIAB  (byte[] buffer)
        {
            SetIAB(buffer, 0, buffer.Length);
        }


        /// <summary>Sets the button on a braille cell with the bits from the byte array</summary>
        public void   SetIAB  (byte[] buffer, int offset, int size)
        {
            for (int i = 0; i < size; i++)
            { 
                for (int b = 0; b < 8; b++)
                { 
                    _keys[ i*8 + b ].IsPressed = ( (buffer[offset+i] & _bit[b]) != 0);
                }
            }
        }



        public void   SetBrailleKeys  (byte value)
        {
            BK1.IsPressed = ((value & 0x01) != 0);   // BrailleKey 1
            BK2.IsPressed = ((value & 0x02) != 0);   // BrailleKey 2
            BK3.IsPressed = ((value & 0x04) != 0);   // BrailleKey 3
            BK4.IsPressed = ((value & 0x08) != 0);   // BrailleKey 4
            BK5.IsPressed = ((value & 0x10) != 0);   // BrailleKey 5
            BK6.IsPressed = ((value & 0x20) != 0);   // BrailleKey 6
            BK7.IsPressed = ((value & 0x40) != 0);   // BrailleKey 7
            BK8.IsPressed = ((value & 0x80) != 0);   // BrailleKey 8
        }

        public void   SetLeftKeys  (byte value)
        {
            LUp.IsPressed         = ((value & 0x01) != 0);   // LUp
            LLeft.IsPressed       = ((value & 0x02) != 0);   // LLeft
            LDown.IsPressed       = ((value & 0x04) != 0);   // LDown
            LRight.IsPressed      = ((value & 0x08) != 0);   // LRight
            LSelect.IsPressed     = ((value & 0x10) != 0);   // LSelect
            LSpecial.IsPressed    = ((value & 0x20) != 0);   // LSpecial
            LT.IsPressed          = ((value & 0x40) != 0);   // LT
            LSpace.IsPressed      = ((value & 0x80) != 0);   // LSpace
        }

        public void   SetRightKeys  (byte value)
        {
            RUp.IsPressed         = ((value & 0x01) != 0);   // RUp
            RLeft.IsPressed       = ((value & 0x02) != 0);   // RLeft
            RDown.IsPressed       = ((value & 0x04) != 0);   // RDown
            RRight.IsPressed      = ((value & 0x08) != 0);   // RRight
            RSelect.IsPressed     = ((value & 0x10) != 0);   // RSelect
            RSpecial.IsPressed    = ((value & 0x20) != 0);   // RSpecial
            RT.IsPressed          = ((value & 0x40) != 0);   // RT
            RSpace.IsPressed      = ((value & 0x80) != 0);   // RSpace
        }



        /// <summary>Sets the keys with a value from a HyperBraille device</summary>
        public void SetHyperBrailleKeys (long value)
        {
            _value = value;

            Debug.Print ("SetHyperBrailleKeys {0:X12}", value);

            LZoomUp.IsPressed     = ((value & 0x0000000000020000) != 0);  // Linke Wippe Up
            LZoomDown.IsPressed   = ((value & 0x0000000000040000) != 0);  // Linke Wippe Down
            LGesture.IsPressed    = ((value & 0x0000000000010000) != 0);  // Linker HyperReader Key (Gestentaste für Touch)

            RZoomUp.IsPressed     = ((value & 0x0000000200000000) != 0);  // Rechte Wippe Up
            RZoomDown.IsPressed   = ((value & 0x0000000400000000) != 0);  // Rechte Wippe Down
            RGesture.IsPressed    = ((value & 0x0000000100000000) != 0);  // Rechter HyperReader Key (Gestentaste für Touch)
            
            //                                XX
            MoveUp.IsPressed      = ((value & 0x0000000004000000) != 0);
            MoveLeft.IsPressed    = ((value & 0x0000000080000000) != 0);
            MoveDown.IsPressed    = ((value & 0x0000000001000000) != 0);
            MoveRight.IsPressed   = ((value & 0x0000000020000000) != 0);

            MoveUp2.IsPressed     = ((value & 0x0000000002000000) != 0);
            MoveLeft2.IsPressed   = ((value & 0x0000000040000000) != 0);
            MoveDown2.IsPressed   = ((value & 0x0000000008000000) != 0);
            MoveRight2.IsPressed  = ((value & 0x0000000010000000) != 0);
            

            LUp.IsPressed         = ((value & 0x0000000000080000) != 0);
            LLeft.IsPressed       = ((value & 0x0000000000800000) != 0);
            LDown.IsPressed       = ((value & 0x0000000000400000) != 0);
            LRight.IsPressed      = ((value & 0x0000000000100000) != 0);
            LSelect.IsPressed     = ((value & 0x0000000000200000) != 0);


            RUp.IsPressed         = ((value & 0x0000000800000000) != 0);
            RLeft.IsPressed       = ((value & 0x0000008000000000) != 0);
            RDown.IsPressed       = ((value & 0x0000004000000000) != 0);
            RRight.IsPressed      = ((value & 0x0000001000000000) != 0);
            RSelect.IsPressed     = ((value & 0x0000002000000000) != 0);



            LT.IsPressed          = ((value & 0x0000000000000002) != 0);
            LSpace.IsPressed      = ((value & 0x0000000000000008) != 0);
            RSpace.IsPressed      = ((value & 0x0000000000000004) != 0);            
            RT.IsPressed          = ((value & 0x0000000000000001) != 0);


            BK1.IsPressed = ((value & 0x0000000000000100) != 0);
            BK2.IsPressed = ((value & 0x0000000000000200) != 0);
            BK3.IsPressed = ((value & 0x0000000000000400) != 0);
            BK4.IsPressed = ((value & 0x0000000000002000) != 0);
            BK5.IsPressed = ((value & 0x0000000000001000) != 0);
            BK6.IsPressed = ((value & 0x0000000000004000) != 0);
            BK7.IsPressed = ((value & 0x0000000000000800) != 0);
            BK8.IsPressed = ((value & 0x0000000000008000) != 0);

        
        }


        /// <summary>Sets the keys with a value from a BD40, BD42 or BD40II device</summary>
        public void SetBDKeys (byte[] ba)
        {
            ClearIAB();   // IAB KeyUp wird nicht gesendet

            // Byte 0: CR (0xFF = none, 0x00 = 1st cell, 0x01 = 2nd cells)
            if (ba[0] != 255)
            { 
                byte value = (byte)(ba[0] + 1);
                SetIAB( value );
            }

            // Byte 1: unused

            // Byte 2
            LUp.IsPressed      = ( ( ba[2] & 0x40 ) != 0 );   // left top                  (gray in BD, black in BD-II)
            LSelect.IsPressed  = ( ( ba[2] & 0x10 ) != 0 );   // left middle only in BD40  (black)
            LDown.IsPressed    = ( ( ba[2] & 0x04 ) != 0 );   // left bottom               (gray in BD, black in BD-II)

            if  ( ( ba[2] & 0x80 ) != 0 )
            {
                // BD42 with synchronious keys
                RUp.IsPressed     = LUp.IsPressed;                      // right top
                RSelect.IsPressed = LSelect.IsPressed;                  // right middle
                RDown.IsPressed   = LDown.IsPressed;                    // right down
            }
            else
            { 
                RUp.IsPressed      = ( ( ba[2] & 0x08 ) != 0 );   // right top                 (gray in BD, black in BD-II)
                RSelect.IsPressed  = ( ( ba[2] & 0x02 ) != 0 );   // right middle only in BD40 (black)
                RDown.IsPressed    = ( ( ba[2] & 0x01 ) != 0 );   // right bottom              (gray in BD, black in BD-II)
            }


            // Byte 3 (only in BD-II)
            LT.IsPressed       = ( ( ba[3] & 0x04 ) != 0 );   // left  outside (cursor Left)
            LSpace.IsPressed   = ( ( ba[3] & 0x40 ) != 0 );   // left  inside  (cursor down)
            RSpace.IsPressed   = ( ( ba[3] & 0x08 ) != 0 );   // right inside  (cursor up)
            RT.IsPressed       = ( ( ba[3] & 0x10 ) != 0 );   // right outside (cursor right)
        }

        /// <summary>Sets the keys with a value from a Flat device. Only IAB and IAB2</summary>
        public void SetFlatKeys (byte[] ba)
        {
            // Byte:     0     1     2     3     4     5     6     7
            // Flat20: 0xFF, 0x28, 0x06, 0x00, 0x00, 0x00, 0x00, 0x01
            // Achtung: Das Flat setzt den LDown und den RSelect.
            // Das Flat hat aber keine Tasten sonst ausser die IABs
            // Darum werden wir nur die IABs aus!

            ClearIAB();   // IAB KeyUp wird nicht gesendet

            // Byte 0: CR (0xFF = none, 0x00 = 1st cell, 0x01 = 2nd cells)
            if (ba[0] != 255)
            { 
                byte value = (byte)(ba[0] + 1);
                SetIAB( value );
            }


        }





        /// <summary>Sets the keys with a value from the Hyperflat device device</summary>
        public void SetHyperflatKeys(byte top, byte bottom, Position position)
        {

            if ( position == Position.Left )
            {
                LLeft.IsPressed     = ( (bottom & 0x02) != 0);      // U = L
                LDown.IsPressed     = ( (bottom & 0x04) != 0);      // L = D
                LRight.IsPressed    = ( (bottom & 0x08) != 0);      // D = R
                LUp.IsPressed       = ( (bottom & 0x01) != 0);      // R = U
                LSelect.IsPressed   = ( (bottom & 0x10) != 0);      // X
                LZoomUp.IsPressed   = ( (bottom & 0x80) != 0);      // +
                LZoomDown.IsPressed = ( (bottom & 0x40) != 0);      // -

                Back.IsPressed      = ( (   top & 0x08) != 0);      // BACK
                Home.IsPressed      = ( (   top & 0x01) != 0);      // HOME
                LSpace.IsPressed    = ( (   top & 0x02) != 0);      // Strg
                LT.IsPressed        = ( (   top & 0x80) != 0);      // Shift
                Esc.IsPressed       = ( (   top & 0x10) != 0);      // ESC
                LGesture.IsPressed  = ( (   top & 0x04) != 0);      // GESTE
            }

            else if ( position == Position.Right )
            {
                LRight.IsPressed    = ( (bottom & 0x02) != 0);      // U = R
                LUp.IsPressed       = ( (bottom & 0x04) != 0);      // L = U
                LLeft.IsPressed     = ( (bottom & 0x08) != 0);      // D = L
                LDown.IsPressed     = ( (bottom & 0x01) != 0);      // R = D
                LSelect.IsPressed   = ( (bottom & 0x10) != 0);      // X
                LZoomDown.IsPressed = ( (bottom & 0x80) != 0);      // + = -
                LZoomUp.IsPressed   = ( (bottom & 0x40) != 0);      // - = +

                LGesture.IsPressed  = ( (   top & 0x08) != 0);      // BACK  = GESTE
                Esc.IsPressed       = ( (   top & 0x01) != 0);      // HOME  = ESC
                LT.IsPressed        = ( (   top & 0x02) != 0);      // Strg  = Shift
                LSpace.IsPressed    = ( (   top & 0x80) != 0);      // Shift = Strg
                Home.IsPressed      = ( (   top & 0x10) != 0);      // ESC   = HOME
                Back.IsPressed      = ( (   top & 0x04) != 0);      // GESTE = BACK
            }

            else if ( position == Position.Rear )
            {
                LDown.IsPressed     = ( (bottom & 0x02) != 0);      // U = D
                LRight.IsPressed    = ( (bottom & 0x04) != 0);      // L = R
                LUp.IsPressed       = ( (bottom & 0x08) != 0);      // D = U
                LLeft.IsPressed     = ( (bottom & 0x01) != 0);      // R = L
                LSelect.IsPressed   = ( (bottom & 0x10) != 0);      // X
                LZoomDown.IsPressed = ( (bottom & 0x80) != 0);      // + = -
                LZoomUp.IsPressed   = ( (bottom & 0x40) != 0);      // - = +

                LGesture.IsPressed  = ( (   top & 0x08) != 0);      // BACK  = GESTE
                Esc.IsPressed       = ( (   top & 0x01) != 0);      // HOME  = ESC
                LT.IsPressed        = ( (   top & 0x02) != 0);      // Strg  = Shift
                LSpace.IsPressed    = ( (   top & 0x80) != 0);      // Shift = Strg
                Home.IsPressed      = ( (   top & 0x10) != 0);      // ESC   = HOME
                Back.IsPressed      = ( (   top & 0x04) != 0);      // GESTE = BACK
            }

            else
            {
                LUp.IsPressed       = ( (bottom & 0x02) != 0);      // U
                LLeft.IsPressed     = ( (bottom & 0x04) != 0);      // L
                LDown.IsPressed     = ( (bottom & 0x08) != 0);      // D
                LRight.IsPressed    = ( (bottom & 0x01) != 0);      // R
                LSelect.IsPressed   = ( (bottom & 0x10) != 0);      // X
                LZoomUp.IsPressed   = ( (bottom & 0x80) != 0);      // +
                LZoomDown.IsPressed = ( (bottom & 0x40) != 0);      // -

                Back.IsPressed      = ( (   top & 0x08) != 0);      // BACK
                Home.IsPressed      = ( (   top & 0x01) != 0);      // HOME
                LSpace.IsPressed    = ( (   top & 0x02) != 0);      // Strg
                LT.IsPressed        = ( (   top & 0x80) != 0);      // Shift
                Esc.IsPressed       = ( (   top & 0x10) != 0);      // ESC
                LGesture.IsPressed  = ( (   top & 0x04) != 0);      // GESTE
            }
        }





        /// <summary>Original value of the keys</summary>
        public long Value       { get { return _value;  } }





        /// <summary>Gibt null oder die Taste zurück die sich an dem Ponkt befindet</summary>
        public DeviceKey   HitTest         (System.Drawing.Point pt)
        {
            foreach(DeviceKey k in _keys)
            {
                if ( k.Contains(pt) )   return k;   // -->
            }

            return null;
        }











        public DeviceKey IAB_01       { get { return _keys[00]; } }
        public DeviceKey IAB_02       { get { return _keys[01]; } }
        public DeviceKey IAB_03       { get { return _keys[02]; } }
        public DeviceKey IAB_04       { get { return _keys[03]; } }
        public DeviceKey IAB_05       { get { return _keys[04]; } }
        public DeviceKey IAB_06       { get { return _keys[05]; } }
        public DeviceKey IAB_07       { get { return _keys[06]; } }
        public DeviceKey IAB_08       { get { return _keys[07]; } }
        public DeviceKey IAB_09       { get { return _keys[08]; } }
        public DeviceKey IAB_10       { get { return _keys[09]; } }
        public DeviceKey IAB_11       { get { return _keys[10]; } }
        public DeviceKey IAB_12       { get { return _keys[11]; } }
        public DeviceKey IAB_13       { get { return _keys[12]; } }
        public DeviceKey IAB_14       { get { return _keys[13]; } }
        public DeviceKey IAB_15       { get { return _keys[14]; } }
        public DeviceKey IAB_16       { get { return _keys[15]; } }

        public DeviceKey IAB2_01      { get { return _keys[100]; } }
        public DeviceKey IAB2_02      { get { return _keys[101]; } }
        public DeviceKey IAB2_03      { get { return _keys[102]; } }
        public DeviceKey IAB2_04      { get { return _keys[103]; } }
        public DeviceKey IAB2_05      { get { return _keys[104]; } }
        public DeviceKey IAB2_06      { get { return _keys[105]; } }
        public DeviceKey IAB2_07      { get { return _keys[106]; } }
        public DeviceKey IAB2_08      { get { return _keys[107]; } }
        public DeviceKey IAB2_09      { get { return _keys[108]; } }
        public DeviceKey IAB2_10      { get { return _keys[109]; } }
        public DeviceKey IAB2_11      { get { return _keys[110]; } }
        public DeviceKey IAB2_12      { get { return _keys[111]; } }
        public DeviceKey IAB2_13      { get { return _keys[112]; } }
        public DeviceKey IAB2_14      { get { return _keys[113]; } }
        public DeviceKey IAB2_15      { get { return _keys[114]; } }
        public DeviceKey IAB2_16      { get { return _keys[115]; } }



        /// <summary>200 BrailleKey 1</summary>
        public DeviceKey BK1         { get { return _keys[200]; } }

        /// <summary>201 BrailleKey 2</summary>
        public DeviceKey BK2         { get { return _keys[201]; } }

        /// <summary>202 BrailleKey 3</summary>
        public DeviceKey BK3         { get { return _keys[202]; } }

        /// <summary>203 BrailleKey 4</summary>
        public DeviceKey BK4         { get { return _keys[203]; } }

        /// <summary>204 BrailleKey 5</summary>
        public DeviceKey BK5         { get { return _keys[204]; } }

        /// <summary>205 BrailleKey 6</summary>
        public DeviceKey BK6         { get { return _keys[205]; } }

        /// <summary>206 BrailleKey 7</summary>
        public DeviceKey BK7         { get { return _keys[206]; } }

        /// <summary>207 BrailleKey 8</summary>
        public DeviceKey BK8         { get { return _keys[207]; } }







        /// <summary>208 Left Up</summary>
        public DeviceKey LUp                 { get { return _keys[208]; } }

        /// <summary>209 Left Left</summary>
        public DeviceKey LLeft               { get { return _keys[209]; } }

        /// <summary>210 Left Down</summary>
        public DeviceKey LDown               { get { return _keys[210]; } }

        /// <summary>211 Left Right</summary>
        public DeviceKey LRight              { get { return _keys[211]; } }

        /// <summary>212 Left Enter</summary>
        public DeviceKey LSelect             { get { return _keys[212]; } }

        /// <summary>213 Left Special. HR Key to activate gesture input</summary>
        public DeviceKey LSpecial            { get { return _keys[213]; } }

        /// <summary>214 Left Thumb (Linke Daumentaste). Wird auch als Shift benutzt.</summary>
        public DeviceKey LT                  { get { return _keys[214]; } }

        /// <summary>215 Left Space (linke Leertaste). Wird auch als Strg benutzt.</summary>
        public DeviceKey LSpace              { get { return _keys[215]; } }







        /// <summary>216 Right Up</summary>
        public DeviceKey RUp                 { get { return _keys[216]; } }

        /// <summary>217 Right Left</summary>
        public DeviceKey RLeft               { get { return _keys[217]; } }

        /// <summary>218 Right Down</summary>
        public DeviceKey RDown               { get { return _keys[218]; } }

        /// <summary>219 Right Right</summary>
        public DeviceKey RRight              { get { return _keys[219]; } }

        /// <summary>220 Right Enter</summary>
        public DeviceKey RSelect             { get { return _keys[220]; } }

        /// <summary>221 Right Special (On/Off)</summary>
        public DeviceKey RSpecial            { get { return _keys[221]; } }

        /// <summary>222 Right Thumb (rechte Daumentaste). Wird auch als Shift benutzt.</summary>
        public DeviceKey RT                  { get { return _keys[222]; } }

        /// <summary>223 Right Space (rechte Leertaste). Wird auch als Strg benutzt.</summary>
        public DeviceKey RSpace              { get { return _keys[223]; } }






        /// <summary>224 Left Zoop Up</summary>
        public DeviceKey LZoomUp             { get { return _keys[224]; } }

        /// <summary>225 Left Zoom Down</summary>
        public DeviceKey LZoomDown           { get { return _keys[225]; } }

        /// <summary>226 Right Zoom Up</summary>
        public DeviceKey RZoomUp             { get { return _keys[226]; } }

        /// <summary>227 Right Zoom Down</summary>
        public DeviceKey RZoomDown           { get { return _keys[227]; } }




        /// <summary>228 Navigation</summary>
        public DeviceKey MoveUp              { get { return _keys[228]; } }

        /// <summary>229 Navigation</summary>
        public DeviceKey MoveLeft            { get { return _keys[229]; } }

        /// <summary>230 Navigation</summary>
        public DeviceKey MoveDown            { get { return _keys[230]; } }

        /// <summary>231 Navigation</summary>
        public DeviceKey MoveRight           { get { return _keys[231]; } }




        /// <summary>232 Navigation 2x</summary>
        public DeviceKey MoveUp2             { get { return _keys[232]; } }

        /// <summary>233 Navigation 2x</summary>
        public DeviceKey MoveLeft2           { get { return _keys[233]; } }

        /// <summary>234 Navigation 2x</summary>
        public DeviceKey MoveDown2           { get { return _keys[234]; } }

        /// <summary>235 Navigation 2x</summary>
        public DeviceKey MoveRight2          { get { return _keys[235]; } }



        /// <summary>236 Abbrechen</summary>
        public DeviceKey Esc                 { get { return _keys[236]; } }

        /// <summary>237 Startbildschirm</summary>
        public DeviceKey Home                { get { return _keys[237]; } }

        /// <summary>238 Zurück-Taste</summary>
        public DeviceKey Back                { get { return _keys[238]; } }


        /// <summary>239 Left Gesture Key for touch input</summary>
        public DeviceKey LGesture            { get { return _keys[239]; } }

        /// <summary>240 Right Gesture Key for touch input</summary>
        public DeviceKey RGesture            { get { return _keys[240]; } }


    }
}