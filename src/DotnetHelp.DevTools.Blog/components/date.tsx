const options: Intl.DateTimeFormatOptions = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };

export default function FormattedDate({ date }: { date: string }) {
    return (
        <span>{new Date(date).toLocaleDateString("en-US", options)}</span>
    );
}