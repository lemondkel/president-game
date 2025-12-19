using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ProfileImageChanger : MonoBehaviour
{
    public RawImage profileDisplay; // 프로필이 표시될 RawImage (Image도 가능)

    // 버튼의 OnClick에 연결할 함수
    public void OnClickChangeProfile()
    {
        // 1. 클래스 이름이 정확한지 확인
        // 2. static 함수이므로 바로 호출 가능
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                // NativeGallery에 포함된 LoadImageAtPath 유틸리티 함수 사용
                Texture2D texture = NativeGallery.LoadImageAtPath(path);
                if (texture != null)
                {
                    profileDisplay.texture = texture;
                }
            }
        }, "프로필 사진 선택", "image/*");
    }

    void UpdateProfileImage(string filePath)
    {
        // 파일을 바이트 배열로 읽기
        byte[] fileData = File.ReadAllBytes(filePath);

        // 새로운 텍스트처 생성
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            // RawImage에 적용
            profileDisplay.texture = tex;

            // 만약 UI Image(Sprite)를 사용한다면 아래 주석 해제
            /*
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            profileDisplay.GetComponent<Image>().sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
            */
        }
    }
}