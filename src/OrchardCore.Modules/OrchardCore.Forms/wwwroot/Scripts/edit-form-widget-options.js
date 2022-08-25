/*
** NOTE: This file is generated by Gulp and should not be edited directly!
** Any changes made directly to this file will be overwritten next time its asset group is processed by Gulp.
*/

document.addEventListener('DOMContentLoaded', function () {
  var selectMenus = document.getElementsByClassName('field-type-select-menu');

  for (var x = 0; x < selectMenus.length; x++) {
    selectMenus[x].addEventListener('change', function (e) {
      var wrapper = e.target.closest('.properties-wrapper');
      var labelTextContainer = wrapper.querySelector('.label-text-container');
      var labelOption = wrapper.querySelector('.field-label-option-select-menu');
      var visibleForInputContainers = wrapper.getElementsByClassName('show-for-input');

      if (e.target.value == 'reset' || e.target.value == 'submit' || e.target.value == 'hidden') {
        for (var i = 0; i < visibleForInputContainers.length; i++) {
          visibleForInputContainers[i].classList.add('d-none');
        }

        labelTextContainer.classList.add('d-none');
      } else {
        for (var _i = 0; _i < visibleForInputContainers.length; _i++) {
          visibleForInputContainers[_i].classList.remove('d-none');
        }

        labelOption.dispatchEvent(new Event('change'));
      }
    });
  }

  var labelOptions = document.getElementsByClassName('field-label-option-select-menu');

  for (var _x = 0; _x < labelOptions.length; _x++) {
    labelOptions[_x].addEventListener('change', function (e) {
      var wrapper = e.target.closest('.properties-wrapper');
      var labelTextContainer = wrapper.querySelector('.label-text-container');
      console.log('label-options changed...', wrapper.length, labelTextContainer.length);

      if (e.target.value != 'None') {
        labelTextContainer.classList.remove('d-none');
      } else {
        labelTextContainer.classList.add('d-none');
      }
    });
  }
});