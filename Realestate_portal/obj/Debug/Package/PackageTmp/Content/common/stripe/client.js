
// A reference to Stripe.js initialized with your real test publishable API key.

//real one 
//var stripe = Stripe("pk_live_51JaSbuLIXevd0ZypmZENKQtmRwjdUk89fyQljpmxCnbwXRbWCYmWstP0VMH1Wu7CPk5tS7TfnRSeEMoOjlfwLhkq00wqo0QMdH");

//for testing only
var stripe = Stripe("pk_test_51JaSbuLIXevd0ZypBVR1e7hNZHdt9By7FcF0egjb51fMfO71nNiTXvTrxvYGx7MaqdexSSv5AKXG5MeqIXoqAlcf00izWB3c5l");

// The items the customer wants to buy
var shippingbtn = document.querySelectorAll(".shipping");
var shippinglabel = document.querySelectorAll(".shippinglbl");
var totallbl = document.querySelectorAll(".totallbl");
var shipping;
var deducted=0.00;


for (var item of shippingbtn) {
    item.addEventListener("change", updateShippingFee);
}


function removeDollarSign(text) {  
    var join=text.substring(1,text.length);    
    return Number(join);
}


function updateShippingFee(e) {       
    shipping = e.target.value;    
    for (var item of shippingbtn) {
        if (e.target.id === item.id) {
            $("#" + item.id).prop('checked', true);
        } else {
            $("#" + item.id).prop('checked', false);
        }
    }

    for (var it of shippinglabel) {    
        it.innerHTML = "$"+e.target.value;      
    }


    for (var it of totallbl) {     
        var total;

        if (deducted > 0) {  
            var retainer = deducted;
            deducted = Number(e.target.value);
            total = removeDollarSign(it.innerHTML) + Number(e.target.value);
            total -= retainer;          
        } else {
            deducted = Number(e.target.value);
            total = removeDollarSign(it.innerHTML) + Number(e.target.value);             
        }           
        it.innerHTML = "$" +total.toFixed(2) ;        
    }
    deducted = Number(e.target.value);
    purchase.items[0].shipping = shipping;
}


var purchase = {
    items: [{ shipping: Number(shipping)}]
};

// Disable the button until we have Stripe set up on the page
document.querySelector("button").disabled = true;

var paymentbtn = document.getElementById("paymentbtn");
paymentbtn.disabled = false;
paymentbtn.onclick = (function () {    
    fetch("https://localhost:44371/PaymentIntentApi/Create", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(purchase)          
    })
        .then(function (result) {
            return result.json();
        })
        .then(function (data) {
            var elements = stripe.elements(
            );
            //  elements.update({ locale: 'en' });
            var style = {
                base: {
                    color: "#32325d",
                    fontFamily: 'Arial, sans-serif',
                    fontSmoothing: "antialiased",
                    fontSize: "16px",
                    "::placeholder": {
                        color: "#32325d"
                    }
                },
                invalid: {
                    fontFamily: 'Arial, sans-serif',
                    color: "#fa755a",
                    iconColor: "#fa755a"
                }
            };

            var card = elements.create("card", { style: style });
            // Stripe injects an iframe into the DOM
            card.mount("#card-element");

            card.on("change", function (event) {
                // Disable the Pay button if there are no card details in the Element
                document.querySelector("button").disabled = event.empty;
                document.querySelector("#card-error").textContent = event.error ? event.error.message : "";
            });

            var form = document.getElementById("payment-form");
            form.addEventListener("submit", function (event) {
                event.preventDefault();
                // Complete payment when the submit button is clicked
                payWithCard(stripe, card, data.clientSecret);
            });
        });
});




// Calls stripe.confirmCardPayment
// If the card requires authentication Stripe shows a pop-up modal to
// prompt the user to enter authentication details without leaving your page.
var payWithCard = function (stripe, card, clientSecret) {
    loading(true);
    stripe
        .confirmCardPayment(clientSecret, {
            payment_method: {
                card: card
            }
        })
        .then(function (result) {
            if (result.error) {
                // Show error to your customer
                showError(result.error.message);
            } else {
                // The payment succeeded!
                orderComplete(result.paymentIntent.id);
                var paymentIntent = { id: result.paymentIntent.id };              
                setTimeout(
                    Swal.fire(
                        'Payment Completed',
                        'Your payment was completed successfully, you will be redirect to a confirmation page.',
                        'success'
                    ),
                    1500);
                fetch("https://localhost:44371/Market/CompleteOrder", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },                    
                    body: JSON.stringify(paymentIntent)
                })
                    .then(function (result) {                       
                        window.location.href = "https://localhost:44371/Market/OrderDetails?OrderID=" + paymentIntent.id + "&nw="+true;
                    });
                    
               
            }
        });
};

/* ------- UI helpers ------- */

// Shows a success message when the payment is complete
var orderComplete = function () {
    loading(false);   
    document.querySelector(".result-message").classList.remove("hidden");
    document.querySelector("button").disabled = true;
};

// Show the customer the error from Stripe if their card fails to charge
var showError = function (errorMsgText) {
    loading(false);
    var errorMsg = document.querySelector("#card-error");
    errorMsg.textContent = errorMsgText;
    setTimeout(function () {
        errorMsg.textContent = "";
    }, 4000);
};

// Show a spinner on payment submission
var loading = function (isLoading) {
    if (isLoading) {
        // Disable the button and show a spinner
        document.querySelector("button").disabled = true;
        document.querySelector("#spinner").classList.remove("hidden");
        document.querySelector("#button-text").classList.add("hidden");
    } else {
        document.querySelector("button").disabled = false;
        document.querySelector("#spinner").classList.add("hidden");
        document.querySelector("#button-text").classList.remove("hidden");
    }
};
