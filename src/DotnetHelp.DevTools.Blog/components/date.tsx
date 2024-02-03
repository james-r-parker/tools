const options: Intl.DateTimeFormatOptions = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };

export default function FormattedDated({ date }: { date: string }) {
    return (
        <span>{new Date(date).toLocaleDateString("en-US", options)}</span>
    );
}