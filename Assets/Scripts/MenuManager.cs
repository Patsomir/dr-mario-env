using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private bool BoardIsValid()
    {
        return Config.width > 1 && Config.virusHeight > 0 && Config.virusCount > 0 && Config.height >= Config.virusHeight && Config.virusCount <= Config.width * Config.virusHeight;
    }

    private bool CyclesAreValid()
    {
        return Config.stepMode || (Config.cycleDuration > 0 && Config.fastCycleDuration > 0);
    }

    private bool RemoteInputIsValid()
    {
        return Config.port > 0;
    }

    private int ParseInteger(string value)
    {
        int parsed;
        try
        {
            parsed = Int32.Parse(value);
        }
        catch (FormatException e)
        {
            print(e.Message);
            return -1;
        }
        return parsed;
    }

    private float ParseFloat(string value)
    {
        float parsed;
        try
        {
            parsed = float.Parse(value);
        }
        catch (FormatException e)
        {
            print(e.Message);
            return -1;
        }
        return parsed;
    }

    public void SetWidth(string width)
    {
        Config.width = ParseInteger(width);
    }

    public void SetHeight(string height)
    {
        Config.height = ParseInteger(height);
    }

    public void SetVirusCount(string count)
    {
        Config.virusCount = ParseInteger(count);
    }

    public void SetVirusHeight(string height)
    {
        Config.virusHeight = ParseInteger(height);
    }

    public void SetRealTimeCycles(bool flag)
    {
        Config.stepMode = !flag;
    }

    public void SetCycleLength(string length)
    {
        Config.cycleDuration = ParseFloat(length);
    }

    public void SetFastCycleLength(string length)
    {
        Config.fastCycleDuration = ParseFloat(length);
    }

    public void SetIp(string ip)
    {
        Config.ip = ip;
    }

    public void SetPort(string port)
    {
        Config.port = ParseInteger(port);
    }

    public void SetManualInput(bool flag)
    {
        Config.enableManualPlay = flag;
    }

    public void SetOnlySingleColorPills(bool flag)
    {
        Config.onlySingleColorPills = flag;
    }

    public void StartGame()
    {
        if(BoardIsValid() && CyclesAreValid() && RemoteInputIsValid())
        {
            SceneManager.LoadScene("Main");
        } 
    }
}
