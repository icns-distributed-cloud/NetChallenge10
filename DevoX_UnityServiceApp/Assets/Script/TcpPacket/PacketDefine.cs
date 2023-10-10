using System;

//TCP Socket Packet Code Class 
namespace CSBaseLib
{
    // 0 ~ 9999
    public enum ERROR_CODE : UInt16
    {
        NONE = 0, // None Error

        // Server Initialize Error
        REDIS_INIT_FAIL = 1,    // Redis Initialize Error

        // Login 
        LOGIN_INVALID_AUTHTOKEN = 1001, // Login failed: wrong token
        ADD_USER_DUPLICATION = 1002,
        REMOVE_USER_SEARCH_FAILURE_USER_ID = 1003,
        USER_AUTH_SEARCH_FAILURE_USER_ID = 1004,
        USER_AUTH_ALREADY_SET_AUTH = 1005,
        LOGIN_ALREADY_WORKING = 1006,
        LOGIN_FULL_USER_COUNT = 1007,

        DB_LOGIN_INVALID_PASSWORD = 1011,
        DB_LOGIN_EMPTY_USER = 1012,
        DB_LOGIN_EXCEPTION = 1013,

        ROOM_ENTER_INVALID_STATE = 1021,
        ROOM_ENTER_INVALID_USER = 1022,
        ROOM_ENTER_ERROR_SYSTEM = 1023,
        ROOM_ENTER_INVALID_ROOM_NUMBER = 1024,
        ROOM_ENTER_FAIL_ADD_USER = 1025,

        EnterRoomAgain = 11001, 
    }

    // 1 ~ 10000
    public enum PACKETID : UInt16
    {
        REQ_RES_TEST_ECHO = 101,

        //유니티 클라
        All_Data_Teacher = 1101, // 선생님이 보낸, 표정, 마이크, 화면 공유 
        All_Data_Student = 1102, // 선생님이 보낸, 표정, 마이크, 화면 공유 

        All_Data_Teacher_List = 1103, // 선생님이 보낸, 표정, 화면 공유 
        All_Data_Student_List = 1104, // 선생님이 보낸, 표정, 마이크, 화면 공유 

        Position = 1105, // 선생님이 보낸 자신의 위치 데이터

        Student_Mike = 1106, // 선생님이 설정하는, 학생의 마이크 On, Off

        Audio_Data = 1108, // 선생님, 학생이 보내는 음성 데이터, 마이크
        Audio_Data_Recive = 1116, // 선생님, 학생이 보내는 음성 데이터, 마이크
        Audio_Data_SoundCard = 1110, // 선생님, 학생이 보내는 음성 데이터, 사운드 카드

        CloseClassRoom = 1109, // 선생님이 학생에게 보내는 강의 종료 패킷

        Teacher_Screen = 1111, // 선생님이 보내는 화면 공유 데이터

        CheckUser = 1112, // 선생님이 보내는 화면 공유 데이터

        Call = 1113, 
        AgreeCall = 1114, 
        Warning = 1115, 


        // Client
        CS_BEGIN = 1001,

        REQ_LOGIN = 1002,
        RES_LOGIN = 1003,
        NTF_MUST_CLOSE = 1005,

        REQ_ROOM_ENTER = 1015,
        RES_ROOM_ENTER = 1016,
        NTF_ROOM_USER_LIST = 1017,
        NTF_ROOM_NEW_USER = 1018,

        REQ_ROOM_LEAVE = 1021,
        RES_ROOM_LEAVE = 1022,
        NTF_ROOM_LEAVE_USER = 1023,

        REQ_ROOM_CHAT = 1026,
        NTF_ROOM_CHAT = 1027,


        REQ_ROOM_DEV_ALL_ROOM_START_GAME = 1091,
        RES_ROOM_DEV_ALL_ROOM_START_GAME = 1092,

        REQ_ROOM_DEV_ALL_ROOM_END_GAME = 1093,
        RES_ROOM_DEV_ALL_ROOM_END_GAME = 1094,

        CS_END = 1100,


        // Server System
        SS_START = 8001,

        NTF_IN_CONNECT_CLIENT = 8011,
        NTF_IN_DISCONNECT_CLIENT = 8012,

        REQ_SS_SERVERINFO = 8021,
        RES_SS_SERVERINFO = 8023,

        REQ_IN_ROOM_ENTER = 8031,
        RES_IN_ROOM_ENTER = 8032,

        NTF_IN_ROOM_LEAVE = 8036,


        // DB 8101 ~ 9000
        REQ_DB_LOGIN = 8101,
        RES_DB_LOGIN = 8102,
    }
}