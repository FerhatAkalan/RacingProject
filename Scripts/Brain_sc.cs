using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Brain_sc : MonoBehaviour
{
    private ANN ann;
    private List<Replay> replayMemory = new List<Replay>();
    
    // Q-Learning parametreleri
    private float discount = 0.99f;             // Gelecek durumların ödüllere etkisi
    private float exploreRate = 100.0f;         // Rastgele hareket seçme olasılığı
    private float maxExploreRate = 100.0f;      // Maksimum olasılık değeri
    private float minExploreRate = 0.01f;       // Minimum olasılık değeri
    private float exploreDecay = 0.0001f;       // Her güncellemede olasılık azalma miktarı
    private int mCapacity = 10000;              // Hafıza kapasitesi
    
    // İstatistikler için
    private int failCount = 0;
    private float maxSurvivalTime = 0;
    private GUIStyle guiStyle = new GUIStyle();

    void Start()
    {
        // 5 giriş (sensörler), 2 çıkış (sağ/sol), 1 gizli katman, 6 nöron
        ann = new ANN(5, 2, 1, 6, 0.02f);
        Time.timeScale = 2.0f; // Oyun hızını biraz düşürelim, çok hızlıydı
        
        // GUI stilini ayarla
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 20; // Font boyutunu küçülttük
        guiStyle.normal.textColor = Color.white;
        guiStyle.fontStyle = FontStyle.Bold;
        guiStyle.alignment = TextAnchor.MiddleLeft;
    }

    public List<double> GetState(Vector2 frontRay, Vector2 frontRightRay, Vector2 frontLeftRay, 
                                Vector2 rightRay, Vector2 leftRay)
    {
        List<double> states = new List<double>();
        states.Add(frontRay.magnitude);
        states.Add(frontRightRay.magnitude);
        states.Add(frontLeftRay.magnitude);
        states.Add(rightRay.magnitude);
        states.Add(leftRay.magnitude);
        return states;
    }

    public int DecideAction(List<double> states)
    {
        List<double> qs = SoftMax(ann.CalcOutput(states));
        double maxQ = qs.Max();
        int maxQIndex = qs.ToList().IndexOf(maxQ);

        // Keşif oranını azalt
        exploreRate = Mathf.Clamp(exploreRate - exploreDecay, minExploreRate, maxExploreRate);

        // Epsilon-greedy stratejisi
        if (Random.Range(0f, 100f) < exploreRate)
        {
            return Random.Range(0, 2);
        }

        return maxQIndex;
    }

    public void AddMemory(List<double> states, double reward)
    {
        if (replayMemory.Count > mCapacity)
            replayMemory.RemoveAt(0);
        replayMemory.Add(new Replay(states, reward));
    }

    public void Train()
    {
        for (int i = replayMemory.Count - 1; i >= 0; i--)
        {
            List<double> toutputsOld = new List<double>();
            List<double> toutputsNew = new List<double>();
            toutputsOld = SoftMax(ann.CalcOutput(replayMemory[i].states));

            double maxQOld = toutputsOld.Max();
            int action = toutputsOld.ToList().IndexOf(maxQOld);

            double feedback;
            if (i == replayMemory.Count - 1 || replayMemory[i].reward < -0.9)
                feedback = replayMemory[i].reward;
            else
            {
                toutputsNew = SoftMax(ann.CalcOutput(replayMemory[i + 1].states));
                maxQOld = toutputsNew.Max();
                feedback = replayMemory[i].reward + discount * maxQOld;
            }

            toutputsOld[action] = feedback;
            ann.Train(replayMemory[i].states, toutputsOld);
        }
    }

    public void UpdateStats(float survivalTime, bool failed = false)
    {
        if (survivalTime > maxSurvivalTime)
            maxSurvivalTime = survivalTime;
        if (failed)
            failCount++;
    }

    void OnGUI()
    {
        GUI.BeginGroup(new Rect(10, 10, 250, 200));
        GUI.Box(new Rect(0, 0, 250, 200), "AI Stats", guiStyle);
        
        // Her label için yeterli boşluk bırakarak yerleştirme
        GUI.Label(new Rect(20, 40, 200, 30), "Fails: " + failCount, guiStyle);
        GUI.Label(new Rect(20, 80, 200, 30), "Explore: " + exploreRate.ToString("F2"), guiStyle);
        GUI.Label(new Rect(20, 120, 200, 30), "Best Time: " + maxSurvivalTime.ToString("F2"), guiStyle);
        GUI.Label(new Rect(20, 160, 200, 30), "Memory: " + replayMemory.Count, guiStyle);
        
        GUI.EndGroup();
    }

    private List<double> SoftMax(List<double> values)
    {
        double max = values.Max();
        float scale = 0.0f;
        
        for (int i = 0; i < values.Count; ++i)
            scale += Mathf.Exp((float)(values[i] - max));
        
        List<double> result = new List<double>();
        for (int i = 0; i < values.Count; ++i)
            result.Add(Mathf.Exp((float)(values[i] - max)) / scale);
        
        return result;
    }

    public void ResetBrain()
    {
        replayMemory.Clear();
    }
}

public class Replay
{
    public List<double> states;
    public double reward;

    public Replay(List<double> s, double r)
    {
        states = new List<double>(s);
        reward = r;
    }
}
