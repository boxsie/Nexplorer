import $ from 'jquery';
import moment from 'moment';

import '../../Style/Components/_date-picker-vue.scss';

class DatePickerVueSelectedDate {
    constructor() {
        this.selectedDayMoment = null;
        this.selectedDateText = '';
        this.isValid = false;
    }
}

export default {
    template: require('../../Markup/date-picker-vue.html'),
    props: ['options', 'unavailableDates', 'isLoading'],
    data: () => {
        return {
            dpOptions: {
                minDate: '',
                maxDate: '',
                allowFuture: true,
                allowPast: true
            },
            dateFormat: 'DD/MM/YYYY',
            monthRows: [],
            monthMoments: [],
            currentMonthOffset: 0,
            showCalendar: false,
            calendarPosition: 'center',
            currentMonthName: 'None',
            selectedFromDate: new DatePickerVueSelectedDate(),
            selectedToDate: new DatePickerVueSelectedDate(),
            selectingFromDate: true,
            errorMessageFrom: '',
            errorMessageTo: ''
        };
    },
    computed: {
        showPrevLink() {
            if (!this.dpOptions.minDate) {
                return true;
            } else {
                return moment(this.dpOptions.minDate, this.dateFormat).isBefore(moment().add(this.currentMonthOffset - 1, 'months'));
            }
        },
        showNextLink() {
            if (!this.dpOptions.maxDate) {
                return true;
            } else {
                return moment(this.dpOptions.maxDate, this.dateFormat).isAfter(moment().add(this.currentMonthOffset, 'months'));
            }
        },
        calendarClass() {
            return `calendar ${this.calendarPosition}`;
        }
    },
    watch: {
        isLoading: (newVal) => {
            if (newVal) {
                $('#Calendar').preloader('show', {
                    position: 'absolute'
                });
            } else {
                $('#Calendar').preloader('hide');
            }
        }
    },
    methods: {
        showCalendarFrom() {
            this.selectingFromDate = true;
            this.$refs.startDate.focus();
            this.showCalendar = true;
            this.calendarPosition = 'left';

        },
        showCalendarTo() {
            this.selectingFromDate = false;
            this.$refs.endDate.focus();
            this.showCalendar = true;
            this.calendarPosition = 'right';
        },
        validateInputFrom(fromMoment) {
            this.errorMessageFrom = '';

            if (!fromMoment) {
                this.errorMessageFrom = 'Please enter a date.';
                return false;
            }

            if (!fromMoment.isValid()) {
                this.errorMessageFrom = `Please enter a valid date in the format ${this.dateFormat}.\r\n`;
                return false;
            }

            if (!this.dpOptions.allowPast && fromMoment.isBefore(moment(), 'day')) {
                this.errorMessageFrom = 'Date cannot be in the past.\r\n';
            }

            if (!this.dpOptions.allowFuture && fromMoment.isAfter(moment(), 'day')) {
                this.errorMessageFrom = 'Date cannot be in the future.\r\n';
            }

            if (this.selectedToDate.selectedDayMoment && this.selectedToDate.selectedDayMoment.isBefore(fromMoment, 'day')) {
                this.errorMessageFrom = 'Start date cannot be after the end date.\r\n';
            }

            if (!this.isAvailable(fromMoment)) {
                this.errorMessageTo = 'Date range contains unavailable dates.\r\n';
            }
            
            return this.errorMessageFrom.length === 0;
        },
        validateInputTo(toMoment) {
            this.errorMessageTo = '';

            if (!toMoment) {
                this.errorMessageTo = 'Please enter a date.';
                return false;
            }

            if (!toMoment.isValid()) {
                this.errorMessageTo = `Please enter a valid date in the format ${this.dateFormat}.\r\n`;
                return false;
            }

            if (!this.dpOptions.allowPast && toMoment.isBefore(moment(), 'day')) {
                this.errorMessageTo = 'Date cannot be in the past.\r\n';
            }

            if (!this.dpOptions.allowFuture && toMoment.isAfter(moment(), 'day')) {
                this.errorMessageTo = 'Date cannot be in the future.\r\n';
            }

            if (this.selectedFromDate.selectedDayMoment && this.selectedFromDate.selectedDayMoment.isAfter(toMoment, 'day')) {
                this.errorMessageTo = 'End date cannot be before the start date.\r\n';
            }

            if (!this.isAvailable(toMoment)) {
                this.errorMessageTo = 'Date range contains unavailable dates.\r\n';
            }

            return this.errorMessageTo.length === 0;
        },
        parseInputFrom() {
            if (!this.selectedFromDate.selectedDateText) {
                this.errorMessageFrom = 'Please enter a start date.';
                return;
            }

            const fromMoment = moment(this.selectedFromDate.selectedDateText, this.dateFormat);

            this.selectDate(fromMoment);
        },
        parseInputTo() {
            if (!this.selectedToDate.selectedDateText) {
                this.errorMessageTo = 'Please enter an end date.';
                return;
            }

            const toMoment = moment(this.selectedToDate.selectedDateText, this.dateFormat);

            this.selectDate(toMoment);
        },
        clickDate(dayMoment) {
            if (!this.isAvailable(dayMoment) || !this.isCurrentMonth(dayMoment)) {
                return;
            }

            this.selectDate(dayMoment);
        },
        selectDate(dayMoment) {
            if (this.selectingFromDate) {
                if (this.validateInputFrom(dayMoment)) {
                    this.selectedFromDate.selectedDateText = dayMoment.format(this.dateFormat);
                    this.selectedFromDate.selectedDayMoment = dayMoment;
                    this.$emit('from-date-update', this.selectedFromDate.selectedDateText);

                    if (!this.selectedToDate.selectedDayMoment) {
                        this.showCalendarTo();
                    }
                }
            } else {
                if (this.validateInputTo(dayMoment)) {
                    this.selectedToDate.selectedDateText = dayMoment.format(this.dateFormat);
                    this.selectedToDate.selectedDayMoment = dayMoment;
                    this.$emit('to-date-update', this.selectedToDate.selectedDateText);

                    this.showCalendar = false;
                }
            }
        },
        isValid() {
            return this.validateInputFrom(this.selectedFromDate.selectedDayMoment) && this.validateInputTo(this.selectedToDate.selectedDayMoment);
        },
        changeMonth(monthOffset) {
            this.currentMonthOffset = monthOffset;
            this.getMonthMoments();
        },
        getMonthMoments() {
            this.monthMoments = [];
            this.monthRows = [];

            const rows = 6;
            const cols = 7;

            const offset = this.currentMonthOffset ? this.currentMonthOffset : 0;

            const currentMonthMoment = moment().add(offset, 'months');
            const currentMonthFirstDate = currentMonthMoment.startOf('month');
            const currentMonthFirstDayIndex = currentMonthFirstDate.day();

            const momentFrom = moment(currentMonthFirstDate).add(-currentMonthFirstDayIndex, 'day');

            for (let i = 0; i < rows; i++) {
                const daysRow = [];

                for (let o = 0; o < cols; o++) {
                    const dayMoment = moment(momentFrom).add(i * cols + o, 'day');

                    daysRow.push(dayMoment);
                    
                    this.monthMoments.push(dayMoment);
                }

                this.monthRows.push(daysRow);
            }
            
            this.currentMonthName = currentMonthMoment.format('MMMM YYYY');
        },
        isAvailable(dayMoment) {
            if (!this.dpOptions.allowPast && dayMoment.isBefore(moment(), 'day')) {
                return false;
            }

            if (!this.dpOptions.allowFuture && dayMoment.isAfter(moment(), 'day')) {
                return false;
            }

            if (this.dpOptions.minDate && dayMoment.isBefore(moment(this.dpOptions.minDate, this.dateFormat), 'day')) {
                return false;
            }

            if (this.dpOptions.maxDate && dayMoment.isAfter(moment(this.dpOptions.maxDate, this.dateFormat), 'day')) {
                return false;
            }
            
            if (!this.selectingFromDate && this.selectedFromDate.selectedDayMoment && dayMoment.isBefore(this.selectedFromDate.selectedDayMoment, 'day')) {
                return false;
            }

            if (this.unavailableDates) {
                for (let i = 0; i < this.unavailableDates.length; i++) {
                    const unavailableDate = this.unavailableDates[i];

                    const startMoment = moment(unavailableDate.startDate, this.dateFormat);
                    const endMoment = moment(unavailableDate.endDate, this.dateFormat);

                    if (dayMoment.isBetween(startMoment, endMoment, 'days', '[]')) {
                        return false;
                    }

                    if (!this.selectingFromDate && this.selectedFromDate.selectedDayMoment) {
                        if (startMoment.isAfter(this.selectedFromDate.selectedDayMoment) && dayMoment.isAfter(startMoment, 'day')) {
                            return false;
                        }
                    }
                }
            }

            return true;
        },
        isReserved(dayMoment) {
            if (this.unavailableDates) {
                for (let i = 0; i < this.unavailableDates.length; i++) {
                    const unavailableDate = this.unavailableDates[i];

                    const startMoment = moment(unavailableDate.startDate, this.dateFormat);
                    const endMoment = moment(unavailableDate.endDate, this.dateFormat);

                    if (dayMoment.isBetween(startMoment, endMoment, 'days', '[]')) {
                        return true;
                    }
                }
            }

            return false;
        },
        isSelected(dayMoment) {
            if (this.selectedFromDate.selectedDayMoment && this.selectedToDate.selectedDayMoment) {
                return dayMoment.isBetween(this.selectedFromDate.selectedDayMoment, this.selectedToDate.selectedDayMoment, 'days', '[]');
            } else if (this.selectedFromDate.selectedDayMoment) {
                return dayMoment.isSame(this.selectedFromDate.selectedDayMoment, 'day');
            } else if (this.selectedToDate.selectedDayMoment) {
                return dayMoment.isSame(this.selectedToDate.selectedDayMoment, 'day');
            }

            return false;
        },
        isCurrentMonth(dayMoment) {
            const offset = this.currentMonthOffset ? this.currentMonthOffset : 0;
            return dayMoment.month() === moment().add(offset, 'months').month();
        },
        getDateClass(dayMoment) {
            const currentMonth = this.isCurrentMonth(dayMoment);
            const className = currentMonth && moment().isSame(dayMoment, 'day') ? 'today' : '';

            if (!currentMonth) {
                return `${className} disabled`;
            }

            if (this.isReserved(dayMoment)) {
                return `${className} reserved`;
            }

            if (!this.isAvailable(dayMoment)) {
                return `${className} unavailable`;
            }

            if (this.isSelected(dayMoment)) {
                return `${className} selected`;
            }

            return `${className} available`;
        },
        clearCalendar() {
            this.selectedFromDate = new DatePickerVueSelectedDate();
            this.selectedToDate = new DatePickerVueSelectedDate();
            this.errorMessageTo = '';
            this.errorMessageFrom = '';
            this.showCalendar = false;
        },
        goToToday() {
            this.currentMonthOffset = 0;
            this.getMonthMoments();
        },
        documentClick(e) {
            if (!this.showCalendar)
                return;

            const calendarEl = this.$refs.calendar;
            const startEl = this.$refs.startDate;
            const endEl = this.$refs.endDate;

            const target = e.target;

            if (calendarEl !== target && !calendarEl.contains(target) &&
                startEl !== target && !startEl.contains(target) &&
                endEl !== target && !endEl.contains(target)) {
                this.showCalendar = false;
            }
        }
    },
    mounted() {
        this.dpOptions = Object.assign({}, this.dpOptions, this.options);

        document.addEventListener('click', this.documentClick);

        this.changeMonth(0);
    }
};
