using System;

namespace Metec.MVBD
{

    /// <summary>Nummern der Tasten (0-240)</summary>
    public enum DeviceKeys
    {
        /// <summary>0 IAB 1</summary>
        IAB_01 =  0,    IAB_02 =  1,    IAB_03 =  2,    IAB_04 =  3,    IAB_05 =  4,    IAB_06 =  5,    IAB_07 =  6,    IAB_08 =  7,    IAB_09 =  8,    IAB_10  =  9,
        IAB_11 = 10,    IAB_12 = 11,    IAB_13 = 12,    IAB_14 = 13,    IAB_15 = 14,    IAB_16 = 15,    IAB_17 = 16,    IAB_18 = 17,    IAB_19 = 18,    IAB_20  = 19,
        IAB_21 = 20,    IAB_22 = 21,    IAB_23 = 22,    IAB_24 = 23,    IAB_25 = 24,    IAB_26 = 25,    IAB_27 = 26,    IAB_28 = 27,    IAB_29 = 28,    IAB_30  = 29,
        IAB_31 = 30,    IAB_32 = 31,    IAB_33 = 32,    IAB_34 = 33,    IAB_35 = 34,    IAB_36 = 35,    IAB_37 = 36,    IAB_38 = 37,    IAB_39 = 38,    IAB_40  = 39,
        IAB_41 = 40,    IAB_42 = 41,    IAB_43 = 42,    IAB_44 = 43,    IAB_45 = 44,    IAB_46 = 45,    IAB_47 = 46,    IAB_48 = 47,    IAB_49 = 48,    IAB_50  = 49,
        IAB_51 = 50,    IAB_52 = 51,    IAB_53 = 52,    IAB_54 = 53,    IAB_55 = 54,    IAB_56 = 55,    IAB_57 = 56,    IAB_58 = 57,    IAB_59 = 58,    IAB_60  = 59,
        IAB_61 = 60,    IAB_62 = 61,    IAB_63 = 62,    IAB_64 = 63,    IAB_65 = 64,    IAB_66 = 65,    IAB_67 = 66,    IAB_68 = 67,    IAB_69 = 68,    IAB_70  = 69,
        IAB_71 = 70,    IAB_72 = 71,    IAB_73 = 72,    IAB_74 = 73,    IAB_75 = 74,    IAB_76 = 75,    IAB_77 = 76,    IAB_78 = 77,    IAB_79 = 78,    IAB_80  = 79,
        IAB_81 = 80,    IAB_82 = 81,    IAB_83 = 82,    IAB_84 = 83,    IAB_85 = 84,    IAB_86 = 85,    IAB_87 = 86,    IAB_88 = 87,    IAB_89 = 88,    IAB_90  = 89,
        IAB_91 = 90,    IAB_92 = 91,    IAB_93 = 92,    IAB_94 = 93,    IAB_95 = 94,    IAB_96 = 95,    IAB_97 = 96,    IAB_98 = 97,    IAB_99 = 98,    IAB_100 = 99,

        /// <summary>100 IAB2 1</summary>
        IAB2_01 = 100,  IAB2_02 = 101,  IAB2_03 = 102,  IAB2_04 = 103,  IAB2_05 = 104,  IAB2_06 = 105,  IAB2_07 = 106,  IAB2_08 = 107,  IAB2_09 = 108,  IAB2_10  = 109,
        IAB2_11 = 110,  IAB2_12 = 111,  IAB2_13 = 112,  IAB2_14 = 113,  IAB2_15 = 114,  IAB2_16 = 115,  IAB2_17 = 116,  IAB2_18 = 117,  IAB2_19 = 118,  IAB2_20  = 119,
        IAB2_21 = 120,  IAB2_22 = 121,  IAB2_23 = 122,  IAB2_24 = 123,  IAB2_25 = 124,  IAB2_26 = 125,  IAB2_27 = 126,  IAB2_28 = 127,  IAB2_29 = 128,  IAB2_30  = 129,
        IAB2_31 = 130,  IAB2_32 = 131,  IAB2_33 = 132,  IAB2_34 = 133,  IAB2_35 = 134,  IAB2_36 = 135,  IAB2_37 = 136,  IAB2_38 = 137,  IAB2_39 = 138,  IAB2_40  = 139,
        IAB2_41 = 140,  IAB2_42 = 141,  IAB2_43 = 142,  IAB2_44 = 143,  IAB2_45 = 144,  IAB2_46 = 145,  IAB2_47 = 146,  IAB2_48 = 147,  IAB2_49 = 148,  IAB2_50  = 149,
        IAB2_51 = 150,  IAB2_52 = 151,  IAB2_53 = 152,  IAB2_54 = 153,  IAB2_55 = 154,  IAB2_56 = 155,  IAB2_57 = 156,  IAB2_58 = 157,  IAB2_59 = 158,  IAB2_60  = 159,
        IAB2_61 = 160,  IAB2_62 = 161,  IAB2_63 = 162,  IAB2_64 = 163,  IAB2_65 = 164,  IAB2_66 = 165,  IAB2_67 = 166,  IAB2_68 = 167,  IAB2_69 = 168,  IAB2_70  = 169,
        IAB2_71 = 170,  IAB2_72 = 171,  IAB2_73 = 172,  IAB2_74 = 173,  IAB2_75 = 174,  IAB2_76 = 175,  IAB2_77 = 176,  IAB2_78 = 177,  IAB2_79 = 178,  IAB2_80  = 179,
        IAB2_81 = 180,  IAB2_82 = 181,  IAB2_83 = 182,  IAB2_84 = 183,  IAB2_85 = 184,  IAB2_86 = 185,  IAB2_87 = 186,  IAB2_88 = 187,  IAB2_89 = 188,  IAB2_90  = 189,
        IAB2_91 = 190,  IAB2_92 = 191,  IAB2_93 = 192,  IAB2_94 = 193,  IAB2_95 = 194,  IAB2_96 = 195,  IAB2_97 = 196,  IAB2_98 = 197,  IAB2_99 = 198,  IAB2_100 = 199,





        /// <summary>200 BrailleKey 1</summary>
        BK1 = 200,

        /// <summary>201 BrailleKey 2</summary>
        BK2 = 201,

        /// <summary>202 BrailleKey 3</summary>
        BK3 = 202,

        /// <summary>203 BrailleKey 4</summary>
        BK4 = 203,

        /// <summary>204 BrailleKey 5</summary>
        BK5 = 204,

        /// <summary>205 BrailleKey 6</summary>
        BK6 = 205,

        /// <summary>206 BrailleKey 7</summary>
        BK7 = 206,

        /// <summary>207 BrailleKey 8</summary>
        BK8 = 207,







        /// <summary>208 Left Up</summary>
        LUp         = 208,

        /// <summary>209 Left Left</summary>
        LLeft       = 209,

        /// <summary>210 Left Down</summary>
        LDown       = 210,

        /// <summary>211 Left Right</summary>
        LRight      = 211,

        /// <summary>212 Left Enter</summary>
        LSelect     = 212,

        /// <summary>213 Left Special. HR Key to activate gesture input</summary>
        LSpecial    = 213,

        /// <summary>214 Left Thumb (Linke Daumentaste)</summary>
        LT          = 214,

        /// <summary>215 Left Space (linke Leertaste)</summary>
        LSpace      = 215,







        /// <summary>216 Right Up</summary>
        RUp         = 216,

        /// <summary>217 Right Left</summary>
        RLeft       = 217,

        /// <summary>218 Right Down</summary>
        RDown       = 218,

        /// <summary>219 Right Right</summary>
        RRight      = 219,

        /// <summary>220 Right Enter</summary>
        RSelect     = 220,

        /// <summary>221 Right Special (On/Off)</summary>
        RSpecial    = 221,

        /// <summary>222 Right Thumb (rechte Daumentaste)</summary>
        RT          = 222,

        /// <summary>223 Right Space (rechte Leertaste)</summary>
        RSpace      = 223,






        /// <summary>224 Left Zoop Up</summary>
        LZoomUp     = 224,

        /// <summary>225 Left Zoom Down</summary>
        LZoomDown   = 225,

        /// <summary>226 Right Zoom Up</summary>
        RZoomUp     = 226,

        /// <summary>227 Right Zoom Down</summary>
        RZoomDown   = 227,




        /// <summary>228 Navigation</summary>
        MoveUp      = 228,

        /// <summary>229 Navigation</summary>
        MoveLeft    = 229,

        /// <summary>230 Navigation</summary>
        MoveDown    = 230,

        /// <summary>231 Navigation</summary>
        MoveRight   = 231,




        /// <summary>232 Navigation 2x</summary>
        MoveUp2     = 232,

        /// <summary>233 Navigation 2x</summary>
        MoveLeft2   = 233,

        /// <summary>234 Navigation 2x</summary>
        MoveDown2   = 234,

        /// <summary>235 Navigation 2x</summary>
        MoveRight2  = 235,




       /// <summary>236 Abbrechen</summary>
        Esc         = 236,

        /// <summary>237 Startbildschirm</summary>
        Home        = 237,

        /// <summary>238 Zurück-Taste</summary>
        Back        = 238,


        /// <summary>239 Left Gesture Key for touch input</summary>
        LGesture    = 239,

        /// <summary>240 Right Gesture Key for touch input</summary>
        RGesture    = 240,

    }
}