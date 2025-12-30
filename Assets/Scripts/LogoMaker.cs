using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LogoMaker : MonoBehaviour
{
    [Header("UI References")]
    public Image baseImage;     // 배경 모양 UI
    public Image symbolImage;   // 문양 UI

    [Header("Assets (Drag & Drop White PNGs)")]
    public List<Sprite> baseSprites;    // 방패, 원, 사각형 등
    public List<Sprite> symbolSprites;  // 칼, 해골, 왕관 등

    [Header("Data (Current State)")]
    public int currentBaseIndex = 0;
    public int currentSymbolIndex = 0;
    public Color currentBaseColor = Color.white;
    public Color currentSymbolColor = Color.white;

    void Start()
    {
        UpdateUI();
    }

    // --- 1. 모양 변경 로직 ---
    public void NextBase(int direction) // +1 or -1
    {
        if (baseSprites == null || baseSprites.Count == 0) return;

        currentBaseIndex += direction;
        // 리스트 범위 순환 (0 -> max -> 0)
        if (currentBaseIndex >= baseSprites.Count) currentBaseIndex = 0;
        if (currentBaseIndex < 0) currentBaseIndex = baseSprites.Count - 1;

        UpdateUI();
    }

    public void NextSymbol(int direction)
    {
        if (symbolSprites == null || symbolSprites.Count == 0) return;

        currentSymbolIndex += direction;
        if (currentSymbolIndex >= symbolSprites.Count) currentSymbolIndex = 0;
        if (currentSymbolIndex < 0) currentSymbolIndex = symbolSprites.Count - 1;

        UpdateUI();
    }

    // --- 2. 색상 변경 로직 ---
    public void SetBaseColor(string hexColor) // 예: "#FF0000"
    {
        if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            currentBaseColor = color;
            UpdateUI();
        }
    }

    public void SetSymbolColor(string hexColor)
    {
        if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            currentSymbolColor = color;
            UpdateUI();
        }
    }

    // UI 갱신
    void UpdateUI()
    {
        if (baseSprites != null && baseSprites.Count > 0)
        {
            baseImage.sprite = baseSprites[currentBaseIndex];
            baseImage.color = currentBaseColor;
        }

        if (symbolSprites != null && symbolSprites.Count > 0)
        {
            symbolImage.sprite = symbolSprites[currentSymbolIndex];
            symbolImage.color = currentSymbolColor;
        }
    }

    // --- 3. [핵심] 방법 B: 데이터 저장 및 로드 ---

    // 저장용: 현재 상태를 LogoData 객체로 반환
    public LogoData GetCurrentData()
    {
        return new LogoData
        {
            baseIndex = currentBaseIndex,
            symbolIndex = currentSymbolIndex,
            // '#'을 붙여야 확실하게 Hex로 인식됨
            baseColorHex = "#" + ColorUtility.ToHtmlStringRGB(currentBaseColor),
            symbolColorHex = "#" + ColorUtility.ToHtmlStringRGB(currentSymbolColor)
        };
    }

    // 로드용: 서버에서 받은 LogoData를 UI에 적용
    public void LoadFromData(LogoData data)
    {
        if (data == null) return;

        currentBaseIndex = Mathf.Clamp(data.baseIndex, 0, baseSprites.Count - 1);
        currentSymbolIndex = Mathf.Clamp(data.symbolIndex, 0, symbolSprites.Count - 1);

        if (ColorUtility.TryParseHtmlString(data.baseColorHex, out Color bColor))
            currentBaseColor = bColor;

        if (ColorUtility.TryParseHtmlString(data.symbolColorHex, out Color sColor))
            currentSymbolColor = sColor;

        UpdateUI();
    }
}

[System.Serializable]
public class LogoData
{
    public int baseIndex;
    public int symbolIndex;
    public string baseColorHex;
    public string symbolColorHex;
}