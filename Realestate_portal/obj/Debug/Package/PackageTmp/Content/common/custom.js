// JavaScript Document

//Wizard Init
//FOR WIZARD AND VALIDATIONS

$(document).ready(function () {
    'use strict';

    //defining form
    var form = $("#example-form").show();


    form.validate({
        ignore: ".ignore",
        focusInvalid: true,

        errorPlacement: function errorPlacement(error, element) {
            return false
        },

        //error message block
        showErrors: function (errorMap, errorList) {
            $("#wizard").find("input").each(function () {
                $(this).removeClass("error");
            });
            $(".errorblock").html("");
            if (errorList.length) {
                $(".errorblock").html(errorList[0]['message']);
                $(errorList[0]['element']).addClass("error");
            }
        },

        //rules for validation
        rules: {
            confirm: {
                equalTo: "#password"
            },
            pay: {
                required: true,
            },
            payp: {
                required: '#customRadio2[value="Paypal"]:checked',
            },
            qrcode: {
                required: '#customRadio3[value="Google-Pay"]:checked',
            },
            tnc: {
                required: true,
            },

        },

        //message for validation
        messages: {
            "fname": {
                required: "Firstname required."
            },
            "lname": {
                required: "Lastname required."
            },
            "pnumber": {
                required: "Phone No. required.",
                number: "Invalid number.",
            },
            "email": {
                required: "Email is required.",
                email: "Invalid email."
            },
            "category": {
                required: "Select category.",
            },
            "service": {
                required: "Select service.",
            },
            "consultant": {
                required: "Select consultant."
            },
            "dp": {
                required: "Select dates."
            },
            "pay": {
                required: "Select atleast one mode of payment."
            },
            "payp": {
                required: "Paypal address is required.",
                email: "Please enter a valid email ID."
            },
            "qrcode": {
                required: "Send us your transaction ID."
            },
            "tnc": {
                required: "Accept terms and conditions."
            },
        },


    });

    //wizard steps
    form.children("div").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "none",
        titleTemplate: '#title#',

        //labels
        labels: {
            finish: "Submit",
            next: "Next",
            previous: "Back",
        },

        //while changing step
        onStepChanging: function (event, currentIndex, newIndex) {

            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }

            form.validate().settings.ignore = ":disabled,:hidden";
            return form.valid();
        },

        //while finishing
        onFinishing: function (event, currentIndex) {
            return form.valid();
        },

        //when finished
        onFinished: function (event, currentIndex) {
            console.log("sending informations");
            document.getElementById("loader").style.display = "block";
            //valores de formulario
            var field1 = $('#fname').val();
            var field2 = $('#lname').val();
            var field3 = $('#pnumber').val();
            var field4 = $('#email').val();
            var field5 = "-";//$('#category').val();
            var field6 = "-";//$('#service').val();
            var field8 = $('#dp2').val();
            var field9 = $('#message1').val();
            var field10 = $('input[name="hoursel"]:checked').val();
            //valores de Real state
            var idproperty = $("#idproperty").val();
            var listingtype = $("#listingtype").val();
            var property = $("#property").val();
            var price = $("#price").val();

            $.ajax({
                url: '/Customers/NewLeadfromWeb',
                type: 'POST',
                datatype: 'application/json',
                contentType: 'application/json',
                data: JSON.stringify({
                    idproperty: idproperty,
                    property: property,
                    listingtype: listingtype,
                    price: price,
                    firstname: field1,
                    lastname: field2,
                    email: field4,
                    telephone: field3,
                    date: field8,
                    time: field10,
                    category: field5,
                    service: field6,
                    comments: field9
                }),
                success: function (result) {
                    if (result == "SUCCESS") {
                        $('#example-form')[0].reset();
                        $("#wizard").steps('reset');
                        window.location.href = "/Customers/Success"
                        document.getElementById("loader").style.display = "none";
                    } else {
                        alert("Something went wrong, please try again");
                        document.getElementById("loader").style.display = "none";
                    }
                },
                error: function (err) {

                }
            });
          
         
        }

    });


})

// JavaScript Document

$(document).ready(function () {
    "use strict"

    //varibale for disabling sundays
    var disabledDays = [0];

    //DATE PICKER
    $('#dp').datepicker({

        //options for date picker
        language: 'en',
        multipleDates: 3,
        multipleDatesSeparator: '  ,  ',
        timepicker: true,
        dateTimeSeparator: '-',
        clearButton: true,

        //set min and max time for booking 9=9:00AM, 17=5:59PM
        minHours: 9,
        maxHours: 17,

        // Now can select only dates, which goes after today
        minDate: new Date(),

        // Make Sunday disabled
        onRenderCell: function (date, cellType) {
            if (cellType == 'day') {
                var day = date.getDay(),
                    isDisabled = disabledDays.indexOf(day) != -1;

                return {
                    disabled: isDisabled
                }
            }
        },
    });


    //SELECT2
    $('#category, #service, #consultant').select2();


    //RADIO BUTTON DIV SELECT
    $('input[type="radio"]').click(function () {
        var inputValue = $(this).attr("value");
        var targetBox = $("." + inputValue);
        $(".box").not(targetBox).hide();
        $(targetBox).show();
    });


    //FETCH DATA ON REVIEW SECTION - STEP-4
    $('#fname').change(function () {
        $('#summaryfname').text($(this).val());
    });

    $('#lname').change(function () {
        $('#summarylname').text($(this).val());
    });

    $('#pnumber').change(function () {
        $('#summarypnumber').text($(this).val());
    });

    $('#email').change(function () {
        $('#summaryemail').text($(this).val());
    });

    $('#category').change(function () {
        $('#summarycategory').text($(this).val());
    });

    $('#service').change(function () {
        $('#summaryservice').text($(this).val());
    });

    $('#consultant').change(function () {
        $('#summaryconsultant, #xyz').text($(this).val());
    });

    $('#message1').change(function () {
        $('#summarymessage').text($(this).val());
    });

 


    //////CASCASING SELECT START//////

    //ko binding handler
    ko.bindingHandlers.select2 = {
        init: function (element, valueAccessor) {

            var options = ko.toJS(valueAccessor()) || {};
            setTimeout(function () {
                $(element).select2(options);
            }, 0);
            //$(element).select2(valueAccessor());

            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                $(element).select2('destroy');
            });
        },
        update: function (element) {
            $(element).trigger('change');
        }
    };

    //Define our options model
    var cascadingOption = function (data) {
        var self = this;
        self.data = self;
        self.text = data.text;
        self.childOptions = data.childOptions;
        console.log(self);
    }

    //fill our models with example data
    function buildData() {
        var uomBachelor = new cascadingOption({
            text: 'Manual Therapy',
            childOptions: [
                new cascadingOption({
                    text: 'Dr. Joseph Rio (220$)'
                }),
                new cascadingOption({
                    text: 'Dr. John Mill (220$)'
                })
            ]
        });

        var uomMaster = new cascadingOption({
            text: 'Education and Advice',
            childOptions: [
                new cascadingOption({
                    text: 'Dr. Sandy Andrea (220$)'
                }),
                new cascadingOption({
                    text: 'Dr. Meghan Kate (220$)'
                })
            ]
        });

        var uom = new cascadingOption({
            text: 'Physiotherapist ',
            childOptions: [uomBachelor, uomMaster]
        });

        var mdx = new cascadingOption({
            text: 'Real Estate',
            childOptions: [
                new cascadingOption({
                    text: 'Show Property',
                    childOptions: [{
                        text: 'Dr. Oli Ye (150$)'
                    },
                    {
                        text: 'Dr. Peg Legge (150$)'
                    },
                    ]
                })
                //,
                //new cascadingOption({
                //    text: 'Dental Cleaning',
                //    childOptions: [{
                //        text: 'Dr. Cierra Vega (220$)'
                //    },
                //    {
                //        text: 'Dr. Kierra Gentry (220$)'
                //    }
                //    ]
                //}),
                //new cascadingOption({
                //    text: 'Implants',
                //    childOptions: [{
                //        text: 'Dr. Thomas Crane (220$)'
                //    },
                //    {
                //        text: 'Dr. Miranda Shaffer (220$)'
                //    },
                //    {
                //        text: 'Dr. Alvaro Mcgee (220$)'
                //    }
                //    ]
                //})
            ]
        });


        // Add this part if you need to add more options ---- START


        var h2 = new cascadingOption({
            text: 'Cardiologist',
            childOptions: [
                new cascadingOption({
                    text: 'Checkup',
                    childOptions: [{
                        text: 'Dr. Aida Bugg (150$)'
                    },
                    {
                        text: 'Dr. Peg Legge (150$)'
                    },
                    ]
                }),
                new cascadingOption({
                    text: 'Surgeries',
                    childOptions: [{
                        text: 'Dr. Miranda Shaffer (220$)'
                    },
                    {
                        text: 'Dr. Allie Grater (220$)'
                    },
                    {
                        text: 'Dr. Olive Yew (220$)'
                    },
                    ]
                })
            ]
        });


        // END

        //return values
        return [mdx];

    }

    var viewModel = {
        togaMakers: buildData(),
        selectedCategory: ko.observable(),
        selectedService: ko.observable(),
        selectedConsultant: ko.observable()
    };

    viewModel.togaServices = ko.computed(function () {
        if (viewModel.selectedCategory()) {
            var make = ko.utils.arrayFirst(viewModel.togaMakers, function (item) {
                // console.log(item.text,viewModel.selectedCategory());
                return item.text === viewModel.selectedCategory();
            });
            return make.childOptions;
        }
    });

    viewModel.togaConsultants = ko.computed(function () {
        if (viewModel.selectedService()) {
            var type = ko.utils.arrayFirst(viewModel.togaServices(), function (item) {
                // console.log(item.text,viewModel.selectedService());
                return item.text === viewModel.selectedService();
                console.log("Answer:" + item);
            });
            return type.childOptions;
        }
    });

    ko.applyBindings(viewModel);

    //////CASCASING SELECT END//////

});	//DOCUMENT END//

(function anonymous($context, $element
) {
    with ($context) { with ($data || {}) { return { 'select2': function () { return {} }, 'options': function () { return togaMakers }, 'value': function () { return selectedCategory }, 'optionsValue': function () { return 'text' }, 'optionsText': function () { return 'text' }, 'optionsCaption': function () { return 'Select your Category' }, '_ko_property_writers': function () { return { 'value': function (_z) { selectedCategory = _z } } } } } }
})

    (function anonymous($context, $element
    ) {
        with ($context) { with ($data || {}) { return { 'select2': function () { return {} }, 'options': function () { return togaServices }, 'value': function () { return selectedService }, 'optionsValue': function () { return 'text' }, 'optionsText': function () { return 'text' }, 'optionsCaption': function () { return 'Select your Service' }, 'enable': function () { return togaServices }, '_ko_property_writers': function () { return { 'value': function (_z) { selectedService = _z } } } } } }
    })

    (function anonymous($context, $element
    ) {
        with ($context) { with ($data || {}) { return { 'select2': function () { return {} }, 'options': function () { return togaConsultants }, 'value': function () { return selectedConsultant }, 'optionsValue': function () { return 'text' }, 'optionsText': function () { return 'text' }, 'optionsCaption': function () { return 'Select your Consultant' }, 'enable': function () { return togaConsultants }, '_ko_property_writers': function () { return { 'value': function (_z) { selectedConsultant = _z } } } } } }
    })
