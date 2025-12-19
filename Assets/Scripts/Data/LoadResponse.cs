using System;

[Serializable]
public class LoadResponse
{
    public bool result;
    public string msg;

    // ★ 수정: params는 예약어이므로 앞에 @를 붙여서 변수명으로 사용
    public UserData @params;
}