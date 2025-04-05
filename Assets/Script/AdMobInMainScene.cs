using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// メインシーン用：バナー表示とインタースティシャル初期化
/// </summary>
public class AdMobInMainScene : MonoBehaviour
{
    private bool _isInterstitialReady = false;

    void Start()
    {
        // 初期化（初回だけ実行される仕組み）
        AdmobLibrary.FirstSetting();

        // インタースティシャル準備完了時にフラグON
        AdmobLibrary.OnLoadedInterstitial = () =>
        {
            Debug.Log("🎯 インタースティシャル広告読み込み完了");
            _isInterstitialReady = true;
        };

        // バナー表示
        AdmobLibrary.RequestBanner(AdSize.Banner, AdPosition.Bottom, false);
    }

    public void TryShowInterstitial()
    {
        if (_isInterstitialReady)
        {
            AdmobLibrary.PlayInterstitial();
            _isInterstitialReady = false;

            // 再読み込み（次回に備える）
            AdmobLibrary.RequestInterstitial();
        }
    }

    void OnDestroy()
    {
        AdmobLibrary.DestroyBanner();
        AdmobLibrary.DestroyInterstitial();
    }
}
