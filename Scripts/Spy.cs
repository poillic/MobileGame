using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Analytics;

public class Spy : MonoBehaviour
{
    // Start is called before the first frame update
    async void Awake()
    {
        // On défini l'environnement de travail c'est pour pas mélanger si le projet est en dev ou en prod
        var options = new InitializationOptions();
        options.SetEnvironmentName( "dev" );
        await UnityServices.InitializeAsync( options );

        //Le service d'analytics démarre
        AnalyticsService.Instance.StartDataCollection();

        CustomEvent myEvent = new CustomEvent( "MyEvent" )
        {
            { "someText", "Nice" },
            { "unBool", true },
            { "myInt", 14 },
            { "myFloat", 14.123f }
        };

        AnalyticsService.Instance.RecordEvent( myEvent );

        Analytics.CustomEvent( "superEvent",
            new Dictionary<string, object>
            {
                { "life", 5 },
                { "coins", 10 },
                { "time", Time.realtimeSinceStartup },
                { "name", "superPlayer" }
            }
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
