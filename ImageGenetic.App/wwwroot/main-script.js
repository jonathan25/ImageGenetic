window.onload = function () {
  var currentGeneration
  $("#start-button").prop('disabled', false)
  $('#settings-form').validate({
    submitHandler: function (form) {
      var data = $(form).serializeArray()
      for (var item in data) {
        if (data[item].name == 'minimalAptitude') {
          data[item].value = data[item].value / 100.0;
        }
      }
      $.ajax({
        url: form.action,
        type: form.method,
        data: data,
        success: function (response) {
          clearUI()
          if (response.error) {
            $('#modal-error-body').text(response.message)
            $('#modal-error').modal()
            return
          }

          $("#current-generation-id").val(response.id)
          currentGeneration = response.id
          worker()
        },
        error: function () {
          $('#modal-error-body').text("There was an error while trying to communicate to the server. Try to reload the page and try again.")
          $('#modal-error').modal()
          $("#status").text("Error")
        }
      })
      return false
    }
  })

  $('#load-previous').validate({
    submitHandler: function (form) {
      $.ajax({
        url: form.action + $("#generation-id").val(),
        type: form.method,
        success: function (response) {
          clearUI()
          if (response.error) {
            $('#modal-error-body').text(response.message)
            $('#modal-error').modal()
            return
          }

          $("#current-generation-id").val($("#generation-id").val())
          currentGeneration = $("#generation-id").val()
          worker()
        },
        error: function () {
          $('#modal-error-body').text("There was an error while trying to communicate to the server. Try to reload the page and try again.")
          $('#modal-error').modal()
        }
      })
      return false
    }
  })

  function clearUI() {
    var done = false;
    for (i = 0; i < 20; i++) {
      $("#image-" + (i + 1)).attr("src", "http://via.placeholder.com/1024/f8f9fa/868e96/?text=no%20image");
      $("#aptitude-" + (i + 1)).text("");
    }
    $("#generation-number").text("(not started yet)")
    $("#elapsed").text("0")
    $("#status").text("Not started")
  }

  function worker() {
    var done = false
    $.ajax({
      url: '/api/generate/encoded/' + currentGeneration,
      success: function (data) {
        if (data.error) {
          done = true
          $('#modal-error-body').text(data.message)
          $('#modal-error').modal()
          clearUI()
          $("#status").text("Error")
          return
        }

        done = data.done

        $("#generation-number").text(data.generations)
        $("#elapsed").text(data.secondsElapsed.toFixed(2))
        if (data.timedOut) {
          $("#status").text("Timed out")
        } else {
          $("#status").text(done ? "Done" : "In progress")
        }
        for (i = 0; i < data.images.length; i++) {
          $("#image-" + (i + 1)).attr("src", "data:image/png;base64," + data.images[i].image);
          $("#aptitude-" + (i + 1)).text((data.images[i].aptitude * 100).toFixed(2) + " %");
        }

      },
      complete: function () {
        // Schedule the next request when the current one's complete
        if (!done) {
          setTimeout(worker, 5000)
        } else {

        }
      }
    })
  }

}