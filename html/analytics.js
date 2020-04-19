(function($) {

    $(document).ready(function() {
    		
    		function saveInAnalytics(category, action, label) {
					if ("ga" in window) {
						if (typeof ga.getAll !== "undefined") { 
							tracker = ga.getAll()[0];
					    if (tracker) 
				        tracker.send('event', category, action, label);
						}
					}
    		}

    	  $(".screenshot1").click(function() {
					saveInAnalytics("clicks", "screenshots", "In-game screenshot");
    	  });

    	  $(".screenshot2").click(function() {
					saveInAnalytics("clicks", "screenshots", "Software screenshot");
    	  });

    	  $(".download").click(function() {
					saveInAnalytics("clicks", "download", "pubg maphack v1.2");
    	  });
    	  
    });

}(jQuery));