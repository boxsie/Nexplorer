import moment from 'moment';

export default {
    template: '<span>{{age}}</span>',
    props: ['fromDate', 'precise', 'small'],
    computed: {
        age() {
            const utcMoment = this.$layoutHub.utcMoment;

            return this.precise ? this.formatDuration(moment.duration(utcMoment.diff(this.fromDate))) : moment.duration(utcMoment.diff(this.fromDate)).humanize();
        }
    },
    methods: {
        formatDuration(duration) {
            return `${this.formatDurationUnit(duration.hours(), 'h')} ${this.formatDurationUnit(duration.minutes(), 'm')} ${this.formatDurationUnit(duration.seconds(), 's')}`;
        },
        formatDurationUnit(durationValue, unitName) {
            return durationValue ? `${durationValue}${unitName}` : '';
        }
    }
};